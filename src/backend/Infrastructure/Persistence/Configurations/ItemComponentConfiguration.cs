using ERP.Domain.MasterData;
using ERP.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public sealed class ItemComponentConfiguration : AuditableEntityConfigurationBase<ItemComponent>
{
    protected override void ConfigureAuditableEntity(EntityTypeBuilder<ItemComponent> builder)
    {
        builder.ToTable("item_components");

        builder.Property(entity => entity.ItemId)
            .HasColumnName("item_id")
            .IsRequired();

        builder.Property(entity => entity.ComponentItemId)
            .HasColumnName("component_item_id")
            .IsRequired();

        builder.Property(entity => entity.UomId)
            .HasColumnName("uom_id")
            .IsRequired();

        builder.Property(entity => entity.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.HasOne(entity => entity.ComponentItem)
            .WithMany()
            .HasForeignKey(entity => entity.ComponentItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Uom)
            .WithMany()
            .HasForeignKey(entity => entity.UomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.ItemId, entity.ComponentItemId })
            .IsUnique();
    }
}
