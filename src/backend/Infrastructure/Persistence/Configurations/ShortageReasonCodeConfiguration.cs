using ERP.Domain.Shortages;
using ERP.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public sealed class ShortageReasonCodeConfiguration : AuditableEntityConfigurationBase<ShortageReasonCode>
{
    protected override void ConfigureAuditableEntity(EntityTypeBuilder<ShortageReasonCode> builder)
    {
        builder.ToTable("shortage_reason_codes");

        builder.Property(entity => entity.Code)
            .HasColumnName("code")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(entity => entity.AffectsSupplierBalance)
            .HasColumnName("affects_supplier_balance")
            .IsRequired();

        builder.Property(entity => entity.AffectsStock)
            .HasColumnName("affects_stock")
            .IsRequired();

        builder.Property(entity => entity.RequiresApproval)
            .HasColumnName("requires_approval")
            .IsRequired();

        builder.Property(entity => entity.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(entity => entity.Code)
            .IsUnique();
    }
}
