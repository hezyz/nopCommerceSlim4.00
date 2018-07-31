using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial interface IProductModelFactory
    {
        /// <summary>
        /// Prepare product search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Product search model</returns>
        ProductSearchModel PrepareProductSearchModel(ProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Product list model</returns>
        ProductListModel PrepareProductListModel(ProductSearchModel searchModel);

        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product model</returns>
        ProductModel PrepareProductModel(ProductModel model, Product product, bool excludeProperties = false);

        /// <summary>
        /// Prepare paged related product list model
        /// </summary>
        /// <param name="searchModel">Related product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Related product list model</returns>
        RelatedProductListModel PrepareRelatedProductListModel(RelatedProductSearchModel searchModel, Product product);

        /// <summary>
        /// Prepare related product search model to add to the product
        /// </summary>
        /// <param name="searchModel">Related product search model to add to the product</param>
        /// <returns>Related product search model to add to the product</returns>
        AddRelatedProductSearchModel PrepareAddRelatedProductSearchModel(AddRelatedProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged related product list model to add to the product
        /// </summary>
        /// <param name="searchModel">Related product search model to add to the product</param>
        /// <returns>Related product list model to add to the product</returns>
        AddRelatedProductListModel PrepareAddRelatedProductListModel(AddRelatedProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged associated product list model
        /// </summary>
        /// <param name="searchModel">Associated product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Associated product list model</returns>
        AssociatedProductListModel PrepareAssociatedProductListModel(AssociatedProductSearchModel searchModel, Product product);

        /// <summary>
        /// Prepare associated product search model to add to the product
        /// </summary>
        /// <param name="searchModel">Associated product search model to add to the product</param>
        /// <returns>Associated product search model to add to the product</returns>
        AddAssociatedProductSearchModel PrepareAddAssociatedProductSearchModel(AddAssociatedProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged associated product list model to add to the product
        /// </summary>
        /// <param name="searchModel">Associated product search model to add to the product</param>
        /// <returns>Associated product list model to add to the product</returns>
        AddAssociatedProductListModel PrepareAddAssociatedProductListModel(AddAssociatedProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged product picture list model
        /// </summary>
        /// <param name="searchModel">Product picture search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product picture list model</returns>
        ProductPictureListModel PrepareProductPictureListModel(ProductPictureSearchModel searchModel, Product product);

        /// <summary>
        /// Prepare product tag search model
        /// </summary>
        /// <param name="searchModel">Product tag search model</param>
        /// <returns>Product tag search model</returns>
        ProductTagSearchModel PrepareProductTagSearchModel(ProductTagSearchModel searchModel);

        /// <summary>
        /// Prepare paged product tag list model
        /// </summary>
        /// <param name="searchModel">Product tag search model</param>
        /// <returns>Product tag list model</returns>
        ProductTagListModel PrepareProductTagListModel(ProductTagSearchModel searchModel);

        /// <summary>
        /// Prepare product tag model
        /// </summary>
        /// <param name="model">Product tag model</param>
        /// <param name="productTag">Product tag</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product tag model</returns>
        ProductTagModel PrepareProductTagModel(ProductTagModel model, ProductTag productTag, bool excludeProperties = false);
        
        /// <summary>
        /// Prepare bulk edit product search model
        /// </summary>
        /// <param name="searchModel">Bulk edit product search model</param>
        /// <returns>Bulk edit product search model</returns>
        BulkEditProductSearchModel PrepareBulkEditProductSearchModel(BulkEditProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged bulk edit product list model
        /// </summary>
        /// <param name="searchModel">Bulk edit product search model</param>
        /// <returns>Bulk edit product list model</returns>
        BulkEditProductListModel PrepareBulkEditProductListModel(BulkEditProductSearchModel searchModel);
    }
}