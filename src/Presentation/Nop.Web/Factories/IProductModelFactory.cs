﻿using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the interface of the product model factory
    /// </summary>
    public partial interface IProductModelFactory
    {
        /// <summary>
        /// Get the product template view path
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>View path</returns>
        string PrepareProductTemplateViewPath(Product product);

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>Collection of product overview model</returns>
        IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePictureModel = true, int? productThumbPictureSize = null);

        /// <summary>
        /// Prepare the product details model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Product details model</returns>
        ProductDetailsModel PrepareProductDetailsModel(Product product, bool isAssociatedProduct = false);

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="model">Product reviews model</param>
        /// <param name="product">Product</param>
        /// <returns>Product reviews model</returns>
        ProductReviewsModel PrepareProductReviewsModel(ProductReviewsModel model, Product product);

        /// <summary>
        /// Prepare the customer product reviews model
        /// </summary>
        /// <param name="page">Number of items page; pass null to load the first page</param>
        /// <returns>Customer product reviews model</returns>
        CustomerProductReviewsModel PrepareCustomerProductReviewsModel(int? page);

        /// <summary>
        /// Prepare the product email a friend model
        /// </summary>
        /// <param name="model">Product email a friend model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <returns>product email a friend model</returns>
        ProductEmailAFriendModel PrepareProductEmailAFriendModel(ProductEmailAFriendModel model, Product product, bool excludeProperties);
    }
}
