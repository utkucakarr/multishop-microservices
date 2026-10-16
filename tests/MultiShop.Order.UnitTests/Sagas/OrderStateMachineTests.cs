using MultiShop.Contracts.Cargo;
using MultiShop.Contracts.Catalog;
using MultiShop.Contracts.Orders;
using MultiShop.Contracts.Payments;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Domain.Enums;

namespace MultiShop.Order.UnitTests.Sagas
{
    // Sipariş saga'sının durum geçişleri ve gönderdiği komutlar (docs/02 S-1…S-11, K-1…K-7).
    public class OrderStateMachineTests : IAsyncLifetime
    {
        private OrderSagaTestContext _test = null!;
        private readonly Guid _orderNumber = Guid.NewGuid();

        public async Task InitializeAsync() => _test = await OrderSagaTestContext.StartAsync();

        public async Task DisposeAsync() => await _test.DisposeAsync();

        [Fact]
        public async Task Checkout_creates_the_order_and_asks_catalog_to_reserve_stock()
        {
            await _test.CheckoutAsync(_orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Pending));

            var ordering = _test.Ordering(_orderNumber);
            ordering.Status.Should().Be(OrderStatus.Pending);
            ordering.UserId.Should().Be(OrderSagaTestContext.UserId);
            ordering.TotalPrice.Should().Be(295m);
            ordering.OrderDetails.Should().HaveCount(2);
            ordering.OrderDetails.Should().ContainSingle(x => x.ProductId == "product-1" && x.ProductAmount == 2 && x.ProductTotalPrice == 200m);

            var reserve = _test.Harness.Sent.Select<ReserveStock>().Single().Context.Message;
            reserve.OrderNumber.Should().Be(_orderNumber);
            reserve.Items.Should().BeEquivalentTo(new[] { new ReserveStockItem("product-1", 2), new ReserveStockItem("product-2", 1) });
        }

        [Fact]
        public async Task Reserved_stock_moves_the_order_to_awaiting_payment()
        {
            await _test.CheckoutAsync(_orderNumber);

            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.AwaitingPayment));
            _test.Ordering(_orderNumber).Status.Should().Be(OrderStatus.AwaitingPayment);
            var awaiting = _test.Harness.Published.Select<OrderAwaitingPayment>().Single().Context.Message;
            awaiting.OrderingId.Should().Be(_test.Ordering(_orderNumber).OrderingId);
            awaiting.TotalPrice.Should().Be(295m, "tahsil edilecek tutar saga'dan gelir, istekten değil");
        }

        [Fact]
        public async Task Failed_reservation_cancels_the_order_without_releasing_stock()
        {
            await _test.CheckoutAsync(_orderNumber);

            await _test.PublishAndWaitAsync(new StockReservationFailed(_orderNumber, "Kazak için yeterli stok yok."), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Cancelled));
            var ordering = _test.Ordering(_orderNumber);
            ordering.Status.Should().Be(OrderStatus.Cancelled);
            ordering.FailMessage.Should().Be("Kazak için yeterli stok yok.");
            (await _test.Harness.Published.Any<OrderCancelled>()).Should().BeTrue();
            (await _test.Harness.Sent.Any<ReleaseStock>()).Should().BeFalse("hiçbir satır ayrılmadı, bırakılacak stok yok");
        }

        [Fact]
        public async Task Payment_can_be_retried_until_the_third_failure_cancels_the_order()
        {
            await _test.CheckoutAsync(_orderNumber);
            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);

            await _test.PublishAndWaitAsync(new PaymentFailed(_orderNumber, "Yetersiz bakiye"), _orderNumber, count: 1);
            await _test.PublishAndWaitAsync(new PaymentFailed(_orderNumber, "Kart limiti"), _orderNumber, count: 2);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.AwaitingPayment));
            _test.Instance(_orderNumber).PaymentAttempts.Should().Be(2);
            _test.Ordering(_orderNumber).FailMessage.Should().Be("Kart limiti");
            (await _test.Harness.Published.Any<OrderCancelled>()).Should().BeFalse();

            await _test.PublishAndWaitAsync(new PaymentFailed(_orderNumber, "Kart reddedildi"), _orderNumber, count: 3);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Cancelled));
            _test.Ordering(_orderNumber).Status.Should().Be(OrderStatus.Cancelled);
            _test.Ordering(_orderNumber).FailMessage.Should().Contain($"{OrderStateMachine.MaxPaymentAttempts} kez").And.Contain("Kart reddedildi");
            (await _test.Harness.Sent.Any<ReleaseStock>(x => x.Context.Message.OrderNumber == _orderNumber)).Should().BeTrue();
            (await _test.Harness.Published.Any<OrderCancelled>(x => x.Context.Message.OrderNumber == _orderNumber)).Should().BeTrue();
        }

        [Fact]
        public async Task Successful_payment_commits_stock_and_starts_the_shipment()
        {
            await _test.CheckoutAsync(_orderNumber);
            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);

            await _test.PublishAndWaitAsync(new PaymentSucceeded(_orderNumber, 295m, DateTime.UtcNow), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Paid));
            _test.Ordering(_orderNumber).Status.Should().Be(OrderStatus.Paid);
            (await _test.Harness.Sent.Any<CommitStock>(x => x.Context.Message.OrderNumber == _orderNumber)).Should().BeTrue();

            var shipment = _test.Harness.Sent.Select<CreateShipment>().Single().Context.Message;
            shipment.Recipient.Name.Should().Be("Ayşe");
            shipment.Recipient.City.Should().Be("İstanbul");
            shipment.Recipient.AddressLine2.Should().BeNull("boş ikinci adres satırı gönderilmez");

            var paid = _test.Harness.Published.Select<OrderPaid>().Single().Context.Message;
            paid.CustomerEmail.Should().Be("ayse@example.com");
        }

        [Fact]
        public async Task Stock_reserved_after_cancellation_is_released_again()
        {
            await _test.CheckoutAsync(_orderNumber);

            // İptal, Catalog rezervasyonu bitirmeden geldi.
            await _test.PublishAndWaitAsync(new CancelOrder(_orderNumber, "Müşteri iptal etti."), _orderNumber);
            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Cancelled));

            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Cancelled));
            _test.Harness.Sent.Select<ReleaseStock>().Should().HaveCount(2, "iptalde bir kez, geç gelen rezervasyonda bir kez daha (telafi)");
        }

        [Fact]
        public async Task Payment_arriving_after_cancellation_does_not_reopen_the_order()
        {
            await _test.CheckoutAsync(_orderNumber);
            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);
            await _test.PublishAndWaitAsync(new CancelOrder(_orderNumber, "Admin iptal etti."), _orderNumber);

            await _test.PublishAndWaitAsync(new PaymentSucceeded(_orderNumber, 295m, DateTime.UtcNow), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Cancelled), "iade gerekir, saga yalnızca loglar");
            (await _test.Harness.Sent.Any<CommitStock>()).Should().BeFalse();
            (await _test.Harness.Sent.Any<CreateShipment>()).Should().BeFalse();
        }

        [Fact]
        public async Task Dispatched_and_delivered_shipment_completes_the_order()
        {
            await PayAsync();

            await _test.PublishAndWaitAsync(new ShipmentDispatched(_orderNumber, "MS00000001", "Aras Kargo", OrderSagaTestContext.UserId,
                "Ayşe Yılmaz", "ayse@example.com", DateTime.UtcNow), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Shipped));
            _test.Ordering(_orderNumber).TrackingNumber.Should().Be("MS00000001");
            _test.Ordering(_orderNumber).CargoCompanyName.Should().Be("Aras Kargo");

            await _test.PublishAndWaitAsync(new ShipmentDelivered(_orderNumber, "MS00000001", DateTime.UtcNow), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Completed));
            _test.Ordering(_orderNumber).Status.Should().Be(OrderStatus.Completed);
        }

        [Fact]
        public async Task Delivery_processed_before_dispatch_still_completes_the_order()
        {
            await PayAsync();

            await _test.PublishAndWaitAsync(new ShipmentDelivered(_orderNumber, "MS00000001", DateTime.UtcNow), _orderNumber);

            _test.Instance(_orderNumber).CurrentState.Should().Be(nameof(OrderStateMachine.Completed));
            _test.Ordering(_orderNumber).TrackingNumber.Should().Be("MS00000001");
        }

        [Fact]
        public async Task Second_checkout_with_the_same_order_number_is_ignored()
        {
            await _test.CheckoutAsync(_orderNumber);

            await _test.PublishAndWaitAsync(OrderSagaTestContext.Checkout(_orderNumber, addressId: 1), _orderNumber, count: 2);

            _test.Orderings.Items.Should().ContainSingle(x => x.OrderNumber == _orderNumber);
            _test.Harness.Sent.Select<ReserveStock>().Should().HaveCount(1);
        }

        private async Task PayAsync()
        {
            await _test.CheckoutAsync(_orderNumber);
            await _test.PublishAndWaitAsync(new StockReserved(_orderNumber), _orderNumber);
            await _test.PublishAndWaitAsync(new PaymentSucceeded(_orderNumber, 295m, DateTime.UtcNow), _orderNumber);
        }
    }
}
