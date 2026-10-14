using MassTransit;
using MultiShop.Contracts.Baskets;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;
using MultiShop.Order.Domain.Enums;

namespace MultiShop.Order.Application.Sagas.Activities
{
    /// <summary>
    /// Saga'nın ilk adımı: sepetten gelen satırlarla siparişi (Orderings + OrderDetails) yazar. Fiyat ve tutarlar
    /// Basket'in sunucuda hesapladığı değerlerdir, istemciden sipariş tutarı alınmaz (ORD-02).
    /// Kayıt saga ile aynı transaction'dadır: mesaj işlenemezse sipariş de yazılmamış olur.
    /// </summary>
    public class CreateOrderingActivity : IStateMachineActivity<OrderState, BasketCheckedOut>
    {
        private readonly IRepository<Ordering> _orderingRepository;

        public CreateOrderingActivity(IRepository<Ordering> orderingRepository)
        {
            _orderingRepository = orderingRepository;
        }

        public async Task Execute(BehaviorContext<OrderState, BasketCheckedOut> context, IBehavior<OrderState, BasketCheckedOut> next)
        {
            var message = context.Message;

            var ordering = await _orderingRepository.CreateAsync(new Ordering
            {
                OrderNumber = message.OrderNumber,
                UserId = message.UserId,
                AddressId = message.AddressId,
                TotalPrice = message.TotalPrice,
                OrderDate = message.CheckedOutAt,
                Status = OrderStatus.Pending,
                OrderDetails = message.Items.Select(item => new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductPrice = item.UnitPrice,
                    ProductAmount = item.Quantity,
                    ProductTotalPrice = item.LineTotal
                }).ToList()
            });

            context.Saga.OrderingId = ordering.OrderingId;

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, BasketCheckedOut, TException> context, IBehavior<OrderState, BasketCheckedOut> next)
            where TException : Exception
            => next.Faulted(context);

        public void Probe(ProbeContext context) => context.CreateScope("create-ordering");

        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    }
}
