using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MultiShop.Contracts.Baskets;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Application.Sagas.Activities;
using MultiShop.Order.Domain.Entities;
using MultiShop.Order.UnitTests.Fakes;

namespace MultiShop.Order.UnitTests.Sagas
{
    /// <summary>
    /// Sipariş saga'sı MassTransit test harness'ında: bellek içi transport ve saga deposu, sahte sipariş/adres depoları.
    /// Order.WebApi'deki kayıtların test karşılığı (EF saga deposu ve outbox yerine bellek içi).
    /// </summary>
    public sealed class OrderSagaTestContext : IAsyncDisposable
    {
        public const string UserId = "user-1";

        private readonly IServiceProvider _provider;

        public ITestHarness Harness { get; }
        public ISagaStateMachineTestHarness<OrderStateMachine, OrderState> Saga { get; }
        public OrderStateMachine Machine => Saga.StateMachine;
        public InMemoryRepository<Ordering> Orderings { get; } = new(x => x.OrderingId, (x, id) => x.OrderingId = id);
        public InMemoryRepository<Address> Addresses { get; } = new(x => x.AddressId, (x, id) => x.AddressId = id);

        private OrderSagaTestContext(IServiceProvider provider)
        {
            _provider = provider;
            Harness = provider.GetRequiredService<ITestHarness>();
            Saga = Harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        }

        public static async Task<OrderSagaTestContext> StartAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            OrderSagaTestContext? context = null;
            services.AddSingleton<IRepository<Ordering>>(_ => context!.Orderings);
            services.AddSingleton<IRepository<Address>>(_ => context!.Addresses);
            services.AddScoped<CreateOrderingActivity>();
            services.AddScoped<SyncOrderingStatusActivity>();
            services.AddScoped<StartShipmentActivity>();

            services.AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>().InMemoryRepository();
                // "Gönderilmedi" kontrolleri bus bu kadar süre boşta kalınca biter.
                x.SetTestTimeouts(testTimeout: TimeSpan.FromSeconds(15), testInactivityTimeout: TimeSpan.FromSeconds(1));
            });

            context = new OrderSagaTestContext(services.BuildServiceProvider(validateScopes: true));
            await context.Harness.Start();
            return context;
        }

        public Address AddAddress()
        {
            var address = new Address
            {
                UserId = UserId, Name = "Ayşe", SurName = "Yılmaz", Email = "ayse@example.com", Phone = "5550000000",
                Country = "Türkiye", City = "İstanbul", District = "Kadıköy", Detail1 = "Moda Cad. 1", Detail2 = "",
                Description = "", ZipCode = "34710"
            };
            Addresses.CreateAsync(address).GetAwaiter().GetResult();
            return address;
        }

        public static BasketCheckedOut Checkout(Guid orderNumber, int addressId) => new(
            orderNumber,
            UserId,
            addressId,
            new List<BasketCheckedOutItem>
            {
                new("product-1", "Kazak", 100m, 2, 200m),
                new("product-2", "Atkı", 50m, 1, 50m)
            },
            CouponCode: null,
            DiscountRate: 0,
            Subtotal: 250m,
            DiscountAmount: 0m,
            TaxAmount: 25m,
            ShippingFee: 20m,
            TotalPrice: 295m,
            CheckedOutAt: DateTime.UtcNow);

        /// <summary>Mesajı yayınlar ve saga'nın bu siparişe ait <paramref name="count"/>. mesajı işlemesini bekler.</summary>
        public async Task PublishAndWaitAsync<T>(T message, Guid orderNumber, int count = 1) where T : class
        {
            await Harness.Bus.Publish(message);
            (await ConsumedCountAsync<T>(orderNumber, count)).Should().BeTrue($"saga {count} adet {typeof(T).Name} işlemeliydi");
        }

        /// <summary>Saga ilk sepet onayını işleyip siparişi Pending'e alana kadar bekler.</summary>
        public async Task CheckoutAsync(Guid orderNumber)
        {
            var address = AddAddress();
            await PublishAndWaitAsync(Checkout(orderNumber, address.AddressId), orderNumber);
        }

        public OrderState Instance(Guid orderNumber)
            => Saga.Sagas.Contains(orderNumber) ?? throw new InvalidOperationException($"Saga {orderNumber} yok.");

        public Ordering Ordering(Guid orderNumber) => Orderings.Items.Single(x => x.OrderNumber == orderNumber);

        // Harness'ın SelectAsync'i "bus boşta" sinyaliyle biter; o sinyal bir kez tetiklenince sıfırlanmadığı için sonraki
        // beklemeler hemen dönüyordu. Bunun yerine işlenen mesaj sayısı süre dolana kadar yoklanır.
        private async Task<bool> ConsumedCountAsync<T>(Guid orderNumber, int count) where T : class
        {
            var deadline = DateTime.UtcNow + Harness.TestTimeout;
            while (DateTime.UtcNow < deadline)
            {
                if (Saga.Consumed.Select<T>(x => CorrelationOf(x.Context.Message) == orderNumber).Count() >= count)
                    return true;
                await Task.Delay(20);
            }
            return false;
        }

        // Saga'nın dinlediği tüm mesajlarda sipariş numarası OrderNumber alanında.
        private static Guid CorrelationOf<T>(T message) where T : class
            => (Guid)typeof(T).GetProperty("OrderNumber")!.GetValue(message)!;

        public async ValueTask DisposeAsync()
        {
            await Harness.Stop();
            if (_provider is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
        }
    }
}
