using MassTransit;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;
using MultiShop.Order.Domain.Enums;

namespace MultiShop.Order.Application.Sagas.Activities
{
    /// <summary>
    /// Saga'nın durumunu, sebebini ve kargo bilgisini siparişe (Ordering.Status, FailMessage, TrackingNumber) yazar; her geçişten sonra çağrılır.
    /// Saga durumlarının adları OrderStatus değerleriyle aynıdır. API ve WebUI durumu Orderings tablosundan okur.
    /// </summary>
    public class SyncOrderingStatusActivity : IStateMachineActivity<OrderState>
    {
        private readonly IRepository<Ordering> _orderingRepository;

        public SyncOrderingStatusActivity(IRepository<Ordering> orderingRepository)
        {
            _orderingRepository = orderingRepository;
        }

        public async Task Execute(BehaviorContext<OrderState> context, IBehavior<OrderState> next)
        {
            await SyncAsync(context.Saga);
            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<OrderState, T> context, IBehavior<OrderState, T> next) where T : class
        {
            await SyncAsync(context.Saga);
            await next.Execute(context);
        }

        private async Task SyncAsync(OrderState saga)
        {
            saga.UpdatedAt = DateTime.UtcNow;

            var ordering = await _orderingRepository.GetByIdAsync(saga.OrderingId)
                ?? throw new InvalidOperationException($"Saga {saga.CorrelationId} için sipariş bulunamadı (OrderingId {saga.OrderingId}).");

            ordering.Status = Enum.Parse<OrderStatus>(saga.CurrentState);
            ordering.FailMessage = saga.FailReason;
            ordering.TrackingNumber = saga.TrackingNumber;
            ordering.CargoCompanyName = saga.CargoCompanyName;
            await _orderingRepository.UpdateAsync(ordering);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, TException> context, IBehavior<OrderState> next)
            where TException : Exception
            => next.Faulted(context);

        public Task Faulted<T, TException>(BehaviorExceptionContext<OrderState, T, TException> context, IBehavior<OrderState, T> next)
            where T : class
            where TException : Exception
            => next.Faulted(context);

        public void Probe(ProbeContext context) => context.CreateScope("sync-ordering-status");

        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    }
}
