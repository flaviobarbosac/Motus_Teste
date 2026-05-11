using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.SaleId).IsRequired().HasColumnType("uuid");
        builder.Property(i => i.ProductId).IsRequired().HasColumnType("uuid");
        builder.Property(i => i.ProductDescription).IsRequired().HasMaxLength(500);

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.LineTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.IsCancelled).IsRequired();

        builder.HasOne<Sale>()
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
