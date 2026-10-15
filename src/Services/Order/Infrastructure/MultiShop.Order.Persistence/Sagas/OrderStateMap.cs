using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiShop.Order.Application.Sagas;

namespace MultiShop.Order.Persistence.Sagas
{
    /// <summary>OrderStates tablosu. CorrelationId birincil anahtardır (SagaClassMap ekler).</summary>
    public class OrderStateMap : SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.ToTable("OrderStates");

            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(450);
            entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.FailReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasIndex(x => x.OrderingId).IsUnique();
        }
    }
}
