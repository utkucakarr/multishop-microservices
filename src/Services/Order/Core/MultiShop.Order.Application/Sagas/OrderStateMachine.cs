using MassTransit;
using Microsoft.Extensions.Logging;
using MultiShop.Contracts.Baskets;
using MultiShop.Contracts.Cargo;
using MultiShop.Contracts.Catalog;
using MultiShop.Contracts.Orders;
using MultiShop.Contracts.Payments;
using MultiShop.Order.Application.Sagas.Activities;

namespace MultiShop.Order.Application.Sagas
{
    /// <summary>
    /// Sipariş saga'sı (orkestrasyon): sepet onayından teslimata kadar siparişi yönetir, Catalog'a stok, Cargo'ya gönderi
    /// komutlarını gönderir, Payment'a sipariş durumunu yayınlar. Yan işler (bildirim) event'leri kendisi dinler (koreografi).
    /// <code>
    /// BasketCheckedOut ──► Pending ──StockReserved──► AwaitingPayment ──PaymentSucceeded──► Paid ──ShipmentDispatched──► Shipped
    ///                         │                          │  PaymentFailed: deneme +1          (CreateShipment,              │
    ///                         │                          │  3. hatada iptal                     OrderPaid)        ShipmentDelivered
    ///                         └─StockReservationFailed / CancelOrder─┴──► Cancelled (ayrılmış stok bırakılır)          ▼
    ///                                                                                                                Completed
    /// </code>
    /// Her mesajın etkisi (sipariş, saga durumu, gönderilecek mesajlar) tek transaction'da yazılır (EF outbox).
    /// </summary>
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        /// <summary>Bu kadar başarısız ödeme denemesinden sonra sipariş iptal edilir ve stok bırakılır (Payment da yeni deneme kabul etmez).</summary>
        public const int MaxPaymentAttempts = PaymentPolicy.MaxAttempts;

        private static readonly Uri ReserveStockQueue = new($"queue:{CatalogQueues.ReserveStock}");
        private static readonly Uri CommitStockQueue = new($"queue:{CatalogQueues.CommitStock}");
        private static readonly Uri ReleaseStockQueue = new($"queue:{CatalogQueues.ReleaseStock}");

        public State Pending { get; private set; } = null!;
        public State AwaitingPayment { get; private set; } = null!;
        public State Paid { get; private set; } = null!;
        public State Cancelled { get; private set; } = null!;
        public State Shipped { get; private set; } = null!;
        public State Completed { get; private set; } = null!;

        public Event<BasketCheckedOut> BasketCheckedOut { get; private set; } = null!;
        public Event<StockReserved> StockReserved { get; private set; } = null!;
        public Event<StockReservationFailed> StockReservationFailed { get; private set; } = null!;
        public Event<PaymentSucceeded> PaymentSucceeded { get; private set; } = null!;
        public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;
        public Event<CancelOrder> CancelOrder { get; private set; } = null!;
        public Event<ShipmentDispatched> ShipmentDispatched { get; private set; } = null!;
        public Event<ShipmentDelivered> ShipmentDelivered { get; private set; } = null!;

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => BasketCheckedOut, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => StockReserved, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => StockReservationFailed, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => PaymentSucceeded, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => PaymentFailed, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => CancelOrder, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => ShipmentDispatched, e => e.CorrelateById(context => context.Message.OrderNumber));
            Event(() => ShipmentDelivered, e => e.CorrelateById(context => context.Message.OrderNumber));

            Initially(
                When(BasketCheckedOut)
                    .Then(context =>
                    {
                        context.Saga.UserId = context.Message.UserId;
                        context.Saga.TotalPrice = context.Message.TotalPrice;
                        context.Saga.CreatedAt = context.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .Activity(x => x.OfType<CreateOrderingActivity>())
                    .Send(ReserveStockQueue, context => new ReserveStock(
                        context.Message.OrderNumber,
                        context.Message.Items.Select(item => new ReserveStockItem(item.ProductId, item.Quantity)).ToList()))
                    .TransitionTo(Pending));

            During(Pending,
                When(StockReserved)
                    .TransitionTo(AwaitingPayment)
                    .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>())
                    .Publish(context => new OrderAwaitingPayment(
                        context.Saga.CorrelationId, context.Saga.OrderingId, context.Saga.UserId, context.Saga.TotalPrice)),

                // Catalog hiçbir satırı ayırmadı: bırakılacak stok yok.
                Cancel(When(StockReservationFailed)
                    .Then(context => context.Saga.FailReason = context.Message.Reason)),

                // Rezervasyon sürüyor olabilir: ReleaseStock rezervasyon yoksa bir şey yapmaz; rezervasyon sonradan
                // tamamlanırsa gelen StockReserved, Cancelled durumunda stoğu yine bırakır.
                ReleaseStockAndCancel(When(CancelOrder)
                    .Then(context => context.Saga.FailReason = context.Message.Reason)));

            During(AwaitingPayment,
                When(PaymentSucceeded)
                    .Then(context => context.Saga.FailReason = null)
                    .Send(CommitStockQueue, context => new CommitStock(context.Saga.CorrelationId))
                    .TransitionTo(Paid)
                    .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>())
                    .Activity(x => x.OfType<StartShipmentActivity>()),

                // Müşteri aynı sipariş için tekrar deneyebilir; deneme hakkı bitince sipariş iptal edilir (karar 2).
                When(PaymentFailed)
                    .Then(context => context.Saga.PaymentAttempts++)
                    .IfElse(context => context.Saga.PaymentAttempts >= MaxPaymentAttempts,
                        cancel => ReleaseStockAndCancel(cancel
                            .Then(context => context.Saga.FailReason =
                                $"Ödeme {MaxPaymentAttempts} kez başarısız olduğu için sipariş iptal edildi. Son hata: {context.Message.Reason}")),
                        retry => retry
                            .Then(context => context.Saga.FailReason = context.Message.Reason)
                            .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>())),

                ReleaseStockAndCancel(When(CancelOrder)
                    .Then(context => context.Saga.FailReason = context.Message.Reason)),

                Ignore(StockReserved));

            During(Cancelled,
                // İptal, rezervasyon tamamlanmadan geldi: sonradan ayrılan stok bırakılır (telafi).
                When(StockReserved)
                    .Send(ReleaseStockQueue, context => new ReleaseStock(context.Saga.CorrelationId)),

                // İptal ile ödeme aynı anda oldu: para çekildi ama sipariş iptal. Kendiliğinden düzelmez, iade gerekir.
                When(PaymentSucceeded)
                    .Then(context => logger.LogError(
                        "İptal edilmiş sipariş {OrderNumber} için {Amount} TL ödeme alındı; iade gerekli.",
                        context.Message.OrderNumber, context.Message.Amount)),

                Ignore(StockReservationFailed),
                Ignore(PaymentFailed),
                Ignore(CancelOrder));

            During(Paid,
                When(ShipmentDispatched)
                    .Then(context =>
                    {
                        context.Saga.TrackingNumber = context.Message.TrackingNumber;
                        context.Saga.CargoCompanyName = context.Message.CargoCompanyName;
                    })
                    .TransitionTo(Shipped)
                    .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>()),

                // Mesaj sırası garanti değil: "teslim edildi", "kargoya verildi"den önce işlenirse sipariş yine tamamlanır.
                When(ShipmentDelivered)
                    .Then(context => context.Saga.TrackingNumber = context.Message.TrackingNumber)
                    .TransitionTo(Completed)
                    .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>()),

                Ignore(StockReserved),
                Ignore(PaymentSucceeded),
                Ignore(PaymentFailed),
                Ignore(CancelOrder));

            During(Shipped,
                When(ShipmentDelivered)
                    .TransitionTo(Completed)
                    .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>()),

                Ignore(ShipmentDispatched),
                Ignore(PaymentSucceeded),
                Ignore(PaymentFailed),
                Ignore(CancelOrder));

            During(Completed,
                Ignore(ShipmentDispatched),
                Ignore(ShipmentDelivered),
                Ignore(PaymentSucceeded),
                Ignore(PaymentFailed),
                Ignore(CancelOrder));

            // Aynı sipariş numarasıyla ikinci sepet onayı: sipariş zaten var. DuringAny, Initial dışındaki tüm durumlar.
            DuringAny(
                Ignore(BasketCheckedOut));
        }

        // Ayrılmış (ya da ayrılmakta olan) stok bırakılır, sonra sipariş iptal edilir.
        private EventActivityBinder<OrderState, T> ReleaseStockAndCancel<T>(EventActivityBinder<OrderState, T> binder) where T : class
            => Cancel(binder.Send(ReleaseStockQueue, context => new ReleaseStock(context.Saga.CorrelationId)));

        // Ortak iptal adımı: durum Cancelled olur, siparişe yazılır ve Payment bekleyen ödemeyi kapatsın diye yayınlanır.
        private EventActivityBinder<OrderState, T> Cancel<T>(EventActivityBinder<OrderState, T> binder) where T : class
            => binder
                .TransitionTo(Cancelled)
                .Activity(x => x.OfInstanceType<SyncOrderingStatusActivity>())
                .Publish(context => new OrderCancelled(context.Saga.CorrelationId, context.Saga.UserId, context.Saga.FailReason!));
    }
}
