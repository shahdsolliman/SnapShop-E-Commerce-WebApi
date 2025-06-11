using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapShop.Core.Entities;

namespace SnapShop.Infrastructure.Data.Config
{
    internal class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            builder.Property(p => p.Description)
                .IsRequired();
            builder.HasOne(p => p.productBrand)
                .WithMany()
                .HasForeignKey(p => p.BrandId);
            builder.HasOne(p => p.productCategory)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

        }
    }
}
