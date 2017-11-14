using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class ProductMap : NopEntityTypeConfiguration<Product>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ProductMap()
        {
            this.ToTable("Product");
            this.HasKey(p => p.Id);
            this.Property(p => p.Name).IsRequired().HasMaxLength(400);
            this.Property(p => p.MetaKeywords).HasMaxLength(400);
            this.Property(p => p.MetaTitle).HasMaxLength(400);

            this.Ignore(p => p.ProductType);

            this.HasMany(p => p.ProductTags)
                .WithMany(pt => pt.Products)
                .Map(m => m.ToTable("Product_ProductTag_Mapping"));
        }
    }
}