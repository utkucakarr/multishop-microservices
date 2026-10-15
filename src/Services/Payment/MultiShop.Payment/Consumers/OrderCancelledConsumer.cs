using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Contracts.Orders;
using MultiShop.Payment.Context;
using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Consumers
{
    /// <summary>
    /// Sipariş iptal edildi: bekleyen ödeme kapatılır, yeni deneme kabul edilmez. Kuyruk: <c>payment-order-cancelled</c>.
    /// Kart o sırada bankadaysa (Processing) kayıt yine Cancelled olur; bankanın sonucu bu durumu değiştirmez.
    /// </summary>
    public class OrderCancelledConsumer : IConsumer<OrderCancelled>
    {
        private readonly PaymentContext _context;

        public OrderCancelledConsumer(PaymentContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            var message = context.Message;
            var now = DateTime.UtcNow;

            var cancelled = await _context.OrderPayments
                .Where(x => x.OrderNumber == message.OrderNumber
                    && (x.Status == OrderPaymentStatus.AwaitingPayment || x.Status == OrderPaymentStatus.Processing))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, OrderPaymentStatus.Cancelled)
                    .SetProperty(x => x.UpdatedAt, now), context.CancellationToken);

            if (cancelled > 0 || await _context.OrderPayments.AnyAsync(x => x.OrderNumber == message.OrderNumber, context.CancellationToken))
                return;

            // Sipariş ödeme beklemeye geçmeden iptal edildi (ör. stok yetersiz) ya da iptal OrderAwaitingPayment'tan önce geldi:
            // iptal kaydı açılır ki geç gelen OrderAwaitingPayment ödenebilir bir kayıt oluşturmasın.
            _context.OrderPayments.Add(new OrderPayment
            {
                OrderNumber = message.OrderNumber,
                UserId = message.UserId,
                Status = OrderPaymentStatus.Cancelled,
                CreatedAt = now,
                UpdatedAt = now
            });
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
