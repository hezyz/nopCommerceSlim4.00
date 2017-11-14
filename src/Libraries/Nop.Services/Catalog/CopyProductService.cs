using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductService : ICopyProductService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IDownloadService _downloadService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productService">pProduct service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="localizedEntityService">Localized entity service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="pictureService">Picture service</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="downloadService">Download service</param>
        /// <param name="urlRecordService">URL record service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        public CopyProductService(IProductService productService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService, 
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICategoryService categoryService, 
            IDownloadService downloadService,
            IUrlRecordService urlRecordService, 
            IStoreMappingService storeMappingService)
        {
            this._productService = productService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._pictureService = pictureService;
            this._categoryService = categoryService;
            this._downloadService = downloadService;
            this._urlRecordService = urlRecordService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Copy associated products
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isPublished">A value indicating whether they should be published</param>
        /// <param name="copyImages">A value indicating whether to copy images</param>
        /// <param name="copyAssociatedProducts">A value indicating whether to copy associated products</param>
        /// <param name="productCopy">New product</param>
        protected virtual void CopyAssociatedProducts(Product product, bool isPublished, bool copyImages, bool copyAssociatedProducts, Product productCopy)
        {
            if (!copyAssociatedProducts)
                return;

            var associatedProducts = _productService.GetAssociatedProducts(product.Id, showHidden: true);
            foreach (var associatedProduct in associatedProducts)
            {
                var associatedProductCopy = CopyProduct(associatedProduct, $"Copy of {associatedProduct.Name}",
                    isPublished, copyImages, false);
                associatedProductCopy.ParentGroupedProductId = productCopy.Id;
                _productService.UpdateProduct(productCopy);
            }
        }

        /// <summary>
        /// Copy related products mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        protected virtual void CopyRelatedProductsMapping(Product product, Product productCopy)
        {
            foreach (var relatedProduct in _productService.GetRelatedProductsByProductId1(product.Id, true))
            {
                _productService.InsertRelatedProduct(
                    new RelatedProduct
                    {
                        ProductId1 = productCopy.Id,
                        ProductId2 = relatedProduct.ProductId2,
                        DisplayOrder = relatedProduct.DisplayOrder
                    });
            }
        }

        /// <summary>
        /// Copy category mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        protected virtual void CopyCategoriesMapping(Product product, Product productCopy)
        {
            foreach (var productCategory in product.ProductCategories)
            {
                var productCategoryCopy = new ProductCategory
                {
                    ProductId = productCopy.Id,
                    CategoryId = productCategory.CategoryId,
                    IsFeaturedProduct = productCategory.IsFeaturedProduct,
                    DisplayOrder = productCategory.DisplayOrder
                };

                _categoryService.InsertProductCategory(productCategoryCopy);
            }
        }

        /// <summary>
        /// Copy product pictures
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="newName">New product name</param>
        /// <param name="copyImages"></param>
        /// <param name="productCopy">New product</param>
        /// <returns>Identifiers of old and new pictures</returns>
        protected virtual Dictionary<int, int> CopyProductPictures(Product product, string newName, bool copyImages, Product productCopy)
        {
            //variable to store original and new picture identifiers
            var originalNewPictureIdentifiers = new Dictionary<int, int>();
            if (copyImages)
            {
                foreach (var productPicture in product.ProductPictures)
                {
                    var picture = productPicture.Picture;
                    var pictureCopy = _pictureService.InsertPicture(
                        _pictureService.LoadPictureBinary(picture),
                        picture.MimeType,
                        _pictureService.GetPictureSeName(newName),
                        picture.AltAttribute,
                        picture.TitleAttribute);
                    _productService.InsertProductPicture(new ProductPicture
                    {
                        ProductId = productCopy.Id,
                        PictureId = pictureCopy.Id,
                        DisplayOrder = productPicture.DisplayOrder
                    });
                    originalNewPictureIdentifiers.Add(picture.Id, pictureCopy.Id);
                }
            }
            return originalNewPictureIdentifiers;
        }

        /// <summary>
        /// Copy localization data
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        protected virtual void CopyLocalizationData(Product product, Product productCopy)
        {
            var languages = _languageService.GetAllLanguages(true);

            //localization
            foreach (var lang in languages)
            {
                var name = product.GetLocalized(x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.Name, name, lang.Id);

                var shortDescription = product.GetLocalized(x => x.ShortDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(shortDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.ShortDescription, shortDescription, lang.Id);

                var fullDescription = product.GetLocalized(x => x.FullDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(fullDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.FullDescription, fullDescription, lang.Id);

                var metaKeywords = product.GetLocalized(x => x.MetaKeywords, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaKeywords))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaKeywords, metaKeywords, lang.Id);

                var metaDescription = product.GetLocalized(x => x.MetaDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaDescription, metaDescription, lang.Id);

                var metaTitle = product.GetLocalized(x => x.MetaTitle, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaTitle))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaTitle, metaTitle, lang.Id);

                //search engine name
                _urlRecordService.SaveSlug(productCopy, productCopy.ValidateSeName("", name, false), lang.Id);
            }
        }

        /// <summary>
        /// Copy product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="newName">New product name</param>
        /// <param name="isPublished">A value indicating whether a new product is published</param>
        /// <returns></returns>
        protected virtual Product CopyBaseProductData(Product product, string newName, bool isPublished)
        {
            // product
            var productCopy = new Product
            {
                ProductTypeId = product.ProductTypeId,
                ParentGroupedProductId = product.ParentGroupedProductId,
                VisibleIndividually = product.VisibleIndividually,
                Name = newName,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                ProductTemplateId = product.ProductTemplateId,
                AdminComment = product.AdminComment,
                ShowOnHomePage = product.ShowOnHomePage,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                MetaTitle = product.MetaTitle,
                AllowCustomerReviews = product.AllowCustomerReviews,
                LimitedToStores = product.LimitedToStores,
                MarkAsNew = product.MarkAsNew,
                MarkAsNewStartDateTimeUtc = product.MarkAsNewStartDateTimeUtc,
                MarkAsNewEndDateTimeUtc = product.MarkAsNewEndDateTimeUtc,
                DisplayOrder = product.DisplayOrder,
                Published = isPublished,
                Deleted = product.Deleted,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            //validate search engine name
            _productService.InsertProduct(productCopy);

            //search engine name
            _urlRecordService.SaveSlug(productCopy, productCopy.ValidateSeName("", productCopy.Name, true), 0);
            return productCopy;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a copy of product with all depended data
        /// </summary>
        /// <param name="product">The product to copy</param>
        /// <param name="newName">The name of product duplicate</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
        /// <param name="copyImages">A value indicating whether the product images should be copied</param>
        /// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
        /// <returns>Product copy</returns>
        public virtual Product CopyProduct(Product product, string newName,
            bool isPublished = true, bool copyImages = true, bool copyAssociatedProducts = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("Product name is required");

            var productCopy = CopyBaseProductData(product, newName, isPublished);

            //localization
            CopyLocalizationData(product, productCopy);

            //copy product tags
            foreach (var productTag in product.ProductTags)
            {
                productCopy.ProductTags.Add(productTag);
            }
            _productService.UpdateProduct(productCopy);

            //copy product pictures
            var originalNewPictureIdentifiers = CopyProductPictures(product, newName, copyImages, productCopy);

           
            //product <-> related products mappings
            CopyRelatedProductsMapping(product, productCopy);
            //store mapping
            var selectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(product);
            foreach (var id in selectedStoreIds)
            {
                _storeMappingService.InsertStoreMapping(productCopy, id);
            }

            //associated products
            CopyAssociatedProducts(product, isPublished, copyImages, copyAssociatedProducts, productCopy);

            return productCopy;
        }

        #endregion
    }
}
