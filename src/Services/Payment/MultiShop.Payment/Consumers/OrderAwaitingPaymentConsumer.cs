using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Contracts.Orders;
using MultiShop.Payment.Context;
using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Consumers
{
    /// <summary>
    /// Sipariş stok ayırdı ve ödeme bekliyor: "bu sipariş için şu tutar beklenir" kaydı açılır. Kuyruk: <c>payment-order-awaiting-payment</c>.
    /// Kayıt zaten varsa dokunulmaz: ör. iptal bu mesajdan önce işlendiyse iptal kaydı ödenebilir hale gelmemeli.
    /// </summary>
    public class OrderAwaitingPaymentConsumer : IConsumer<OrderAwaitingPayment>
    {
        private readonly PaymentContext _context;

        public OrderAwaitingPaymentConsumer(PaymentContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderAwaitingPayment> context)
        {
            var message = context.Message;
            if (await _context.OrderPayments.AnyAsync(x => x.OrderNumber == message.OrderNumber, context.CancellationToken))
                return;

            var now = DateTime.UtcNow;
            _context.OrderPayments.Add(new OrderPayment
            {
                OrderNumber = message.OrderNumber,
                OrderingId = message.OrderingId,
                UserId = message.UserId,
                Amount = message.TotalPrice,
                Status = OrderPaymentStatus.AwaitingPayment,
                CreatedAt = now,
                UpdatedAt = now
            });
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
