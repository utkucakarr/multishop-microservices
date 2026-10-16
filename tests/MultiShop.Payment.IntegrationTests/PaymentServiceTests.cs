using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Contracts.Orders;
using MultiShop.Contracts.Payments;
using MultiShop.Payment.Consumers;
using MultiShop.Payment.Context;
using MultiShop.Payment.Dtos.PaymentDtos;
using MultiShop.Payment.Entities;
using MultiShop.Payment.IntegrationTests.Infrastructure;
using MultiShop.Payment.Services.GarantiPosServices;
using MultiShop.Payment.Services.PaymentServices;
using NSubstitute;

namespace MultiShop.Payment.IntegrationTests
{
    /// <summary>
    /// Ödeme kaydının kuralları gerçek SQL Server'da: aynı sipariş için tek deneme (koşullu UPDATE ile "claim"),
    /// deneme hakkı, banka beklenirken gelen iptal, sonucu belirsiz banka hatası. Docker gerekir.
    /// </summary>
    [Collection(SqlServerCollection.Name)]
    public class PaymentServiceTests : IAsyncLifetime
    {
        private const string UserId = "user-1";
        private const decimal Amount = 295.50m;

        private readonly SqlServerFixture _sqlServer;
        private readonly FakePosClient _bank = new();
        private readonly IPublishEndpoint _publish = Substitute.For<IPublishEndpoint>();
        private readonly Guid _orderNumber = Guid.NewGuid();
        private DbContextOptions<PaymentContext> _options = null!;

        public PaymentServiceTests(SqlServerFixture sqlServer)
        {
            _sqlServer = sqlServer;
        }

        public async Task InitializeAsync()
        {
            _options = _sqlServer.Options;
            // Saga stok ayırınca Payment'a OrderAwaitingPayment gelir; kayıt üretimdeki consumer ile açılır.
            await ConsumeAsync(new OrderAwaitingPaymentConsumer(NewContext()), new OrderAwaitingPayment(_orderNumber, 42, UserId, Amount));
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task Successful_payment_charges_the_amount_from_the_order_and_publishes_success()
        {
            var response = await NewService().CreatePaymentAsync(UserId, Card());

            response.IsSuccess.Should().BeTrue();
            response.Status.Should().Be(OrderPaymentStatus.Succeeded);
            _bank.OrderIds.Should().Equal("MLT-SHP-42-1");

            var payment = await LoadPaymentAsync();
            payment.Status.Should().Be(OrderPaymentStatus.Succeeded);
            payment.PaidAt.Should().NotBeNull();
            await _publish.Received(1).Publish(Arg.Is<PaymentSucceeded>(x => x.OrderNumber == _orderNumber && x.Amount == Amount), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Two_simultaneous_payments_for_the_same_order_reach_the_bank_only_once()
        {
            var bankEntered = _bank.HoldNextSale();

            var first = NewService().CreatePaymentAsync(UserId, Card());
            var second = NewService().CreatePaymentAsync(UserId, Card());

            // Biri bankada beklerken diğeri reddedilmiş olmalı.
            var loser = await Task.WhenAny(first, second);
            await bankEntered;
            _bank.Release();

            var winner = loser == first ? second : first;
            await loser.Invoking(t => t).Should().ThrowAsync<ConflictException>();
            (await winner).IsSuccess.Should().BeTrue();
            _bank.OrderIds.Should().HaveCount(1, "çift çekim olmamalı");
            (await CountPaymentInfosAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Third_failed_attempt_closes_the_payment()
        {
            _bank.Result = () => new GarantiPosResult(false, "Yetersiz bakiye");

            var responses = new List<CreatePaymentResponseDto>();
            for (var attempt = 0; attempt < PaymentPolicy.MaxAttempts; attempt++)
                responses.Add(await NewService().CreatePaymentAsync(UserId, Card()));

            responses.Select(x => x.Status).Should().Equal(OrderPaymentStatus.AwaitingPayment, OrderPaymentStatus.AwaitingPayment, OrderPaymentStatus.Failed);
            responses.Select(x => x.RemainingAttempts).Should().Equal(2, 1, 0);
            _bank.OrderIds.Should().Equal(new[] { "MLT-SHP-42-1", "MLT-SHP-42-2", "MLT-SHP-42-3" }, "bankadaki sipariş kimliği her denemede farklı");
            await _publish.Received(3).Publish(Arg.Any<PaymentFailed>(), Arg.Any<CancellationToken>());

            var fourth = () => NewService().CreatePaymentAsync(UserId, Card());
            await fourth.Should().ThrowAsync<ConflictException>().WithMessage("*deneme hakkı doldu*");
            _bank.OrderIds.Should().HaveCount(3);
        }

        [Fact]
        public async Task Cancellation_while_the_bank_is_processing_keeps_the_payment_cancelled()
        {
            var bankEntered = _bank.HoldNextSale();
            var payment = NewService().CreatePaymentAsync(UserId, Card());
            await bankEntered;

            await ConsumeAsync(new OrderCancelledConsumer(NewContext()), new OrderCancelled(_orderNumber, UserId, "Müşteri iptal etti."));
            _bank.Release();

            var response = await payment;
            response.Status.Should().Be(OrderPaymentStatus.Cancelled);
            (await LoadPaymentAsync()).Status.Should().Be(OrderPaymentStatus.Cancelled);
            // Para çekildi: olan bu, saga iade için loglar (S-11).
            await _publish.Received(1).Publish(Arg.Any<PaymentSucceeded>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Another_users_order_is_not_found_and_the_bank_is_not_called()
        {
            var pay = () => NewService().CreatePaymentAsync("someone-else", Card());

            await pay.Should().ThrowAsync<NotFoundException>();
            _bank.OrderIds.Should().BeEmpty();
        }

        [Fact]
        public async Task Bank_error_does_not_count_as_an_attempt_and_the_payment_can_be_retried()
        {
            _bank.Result = () => throw new GarantiPosException("Banka yanıt vermedi.");

            var pay = () => NewService().CreatePaymentAsync(UserId, Card());
            await pay.Should().ThrowAsync<ServiceUnavailableException>();

            var payment = await LoadPaymentAsync();
            payment.Status.Should().Be(OrderPaymentStatus.AwaitingPayment, "kayıt Processing'de takılı kalmamalı");
            payment.FailedAttempts.Should().Be(0);
            (await CountPaymentInfosAsync()).Should().Be(0);
            await _publish.DidNotReceiveWithAnyArgs().Publish<PaymentFailed>(default!, default);

            _bank.Result = () => new GarantiPosResult(true, null);
            (await NewService().CreatePaymentAsync(UserId, Card())).IsSuccess.Should().BeTrue();
            _bank.OrderIds.Should().Equal("MLT-SHP-42-1", "MLT-SHP-42-1");
        }

        [Fact]
        public async Task Only_the_last_four_digits_of_the_card_are_stored()
        {
            await NewService().CreatePaymentAsync(UserId, Card());

            await using var context = NewContext();
            (await context.PaymentInfos.SingleAsync(x => x.OrderNumber == _orderNumber)).CardNumber.Should().Be("**** 8015");
        }

        private CreatePaymentDto Card() => new()
        {
            OrderNumber = _orderNumber,
            CardNumber = "4282209004348015",
            CardOwnerName = "Ayşe Yılmaz",
            Month = "08",
            Year = "2027",
            Cvv = "123"
        };

        private PaymentContext NewContext() => new(_options);

        // Her istek gibi her servis kendi DbContext'ini kullanır (eşzamanlı ödemeler ayrı bağlantılardan gelir).
        private PaymentService NewService() => new(NewContext(), _bank, _publish, NullLogger<PaymentService>.Instance);

        private async Task<OrderPayment> LoadPaymentAsync()
        {
            await using var context = NewContext();
            return await context.OrderPayments.AsNoTracking().SingleAsync(x => x.OrderNumber == _orderNumber);
        }

        private async Task<int> CountPaymentInfosAsync()
        {
            await using var context = NewContext();
            return await context.PaymentInfos.CountAsync(x => x.OrderNumber == _orderNumber);
        }

        private static async Task ConsumeAsync<T>(IConsumer<T> consumer, T message) where T : class
        {
            var context = Substitute.For<ConsumeContext<T>>();
            context.Message.Returns(message);
            context.CancellationToken.Returns(CancellationToken.None);
            await consumer.Consume(context);
        }
    }
}
