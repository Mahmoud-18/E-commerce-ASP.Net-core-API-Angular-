using API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Config;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(x => x.ProductCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.ProductCode).IsUnique();

        builder.Property(x=>x.Category).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x=>x.Price).IsRequired().HasColumnType("decimal");
        builder.Property(x => x.ImagePath).IsRequired();

        builder.Property(x => x.MinimumQuantity).IsRequired();

        builder.Property(x=>x.Id).UseIdentityColumn();
    }
}