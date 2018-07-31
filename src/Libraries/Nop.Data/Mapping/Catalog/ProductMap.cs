using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Represents a product mapping configuration
    /// </summary>
    public partial class ProductMap : NopEntityTypeConfiguration<Product>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable(nameof(Product));
            builder.HasKey(product => product.Id);

            builder.Property(product => product.Name).HasMaxLength(400).IsRequired();
            builder.Property(product => product.MetaKeywords).HasMaxLength(400);
            builder.Property(product => product.MetaTitle).HasMaxLength(400);

            builder.Ignore(product => product.ProductType);

            base.Configure(builder);
        }

        #endregion
    }
}