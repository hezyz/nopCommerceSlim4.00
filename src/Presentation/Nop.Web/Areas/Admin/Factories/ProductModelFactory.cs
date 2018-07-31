using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product model factory implementation
    /// </summary>
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public ProductModelFactory(
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IPictureService pictureService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IStaticCacheManager cacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            VendorSettings vendorSettings)
        {
            this._aclSupportedModelFactory = aclSupportedModelFactory;
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._categoryService = categoryService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._localizedModelFactory = localizedModelFactory;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productTagService = productTagService;
            this._productTemplateService = productTemplateService;
            this._settingModelFactory = settingModelFactory;
            this._cacheManager = cacheManager;
            this._storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            this._storeService = storeService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare copy product model
        /// </summary>
        /// <param name="model">Copy product model</param>
        /// <param name="product">Product</param>
        /// <returns>Copy product model</returns>
        protected virtual CopyProductModel PrepareCopyProductModel(CopyProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = product.Id;
            model.Name = string.Format(_localizationService.GetResource("Admin.Catalog.Products.Copy.Name.New"), product.Name);
            model.Published = true;
            model.CopyImages = true;

            return model;
        }

        /// <summary>
        /// Prepare related product search model
        /// </summary>
        /// <param name="searchModel">Related product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Related product search model</returns>
        protected virtual RelatedProductSearchModel PrepareRelatedProductSearchModel(RelatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare associated product search model
        /// </summary>
        /// <param name="searchModel">Associated product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Associated product search model</returns>
        protected virtual AssociatedProductSearchModel PrepareAssociatedProductSearchModel(AssociatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product picture search model
        /// </summary>
        /// <param name="searchModel">Product picture search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product picture search model</returns>
        protected virtual ProductPictureSearchModel PrepareProductPictureSearchModel(ProductPictureSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare product search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Product search model</returns>
        public virtual ProductSearchModel PrepareProductSearchModel(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Product list model</returns>
        public virtual ProductListModel PrepareProductListModel(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            if (_workContext.CurrentVendor != null)
                searchModel.SearchVendorId = _workContext.CurrentVendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = _categoryService.GetChildCategoryIds(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: categoryIds,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished);

            //prepare list model
            var model = new ProductListModel
            {
                Data = products.Select(product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75);
                    productModel.ProductTypeName = _localizationService.GetLocalizedEnum(product.ProductType);

                    return productModel;
                }),
                Total = products.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product model</returns>
        public virtual ProductModel PrepareProductModel(ProductModel model, Product product, bool excludeProperties = false)
        {
            Action<ProductLocalizedModel, int> localizedModelConfiguration = null;

            if (product != null)
            {
                //fill in model values from the entity
                model = model ?? product.ToModel<ProductModel>();

                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);
                model.ProductTags = string.Join(", ", _productTagService.GetAllProductTagsByProductId(product.Id).Select(tag => tag.Name));

                if (!excludeProperties)
                {
                    model.SelectedCategoryIds = _categoryService.GetProductCategoriesByProductId(product.Id, true)
                        .Select(productCategory => productCategory.CategoryId).ToList();
                }

                //prepare copy product model
                PrepareCopyProductModel(model.CopyProductModel, product);


                //prepare nested search model
                PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
                PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
                PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(product, entity => entity.Name, languageId, false, false);
                    locale.FullDescription = _localizationService.GetLocalized(product, entity => entity.FullDescription, languageId, false, false);
                    locale.ShortDescription = _localizationService.GetLocalized(product, entity => entity.ShortDescription, languageId, false, false);
                    locale.MetaKeywords = _localizationService.GetLocalized(product, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = _localizationService.GetLocalized(product, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = _localizationService.GetLocalized(product, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = _urlRecordService.GetSeName(product, languageId, false, false);
                };
            }

            //set default values for the new model
            if (product == null)
            {
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }

            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare editor settings
            model.ProductEditorSettingsModel = _settingModelFactory.PrepareProductEditorSettingsModel();

            //prepare available product templates
            _baseAdminModelFactory.PrepareProductTemplates(model.AvailableProductTemplates, false);

            //prepare available product types
            var productTemplates = _productTemplateService.GetAllProductTemplates();
            foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
            {
                model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
                foreach (var template in productTemplates)
                {
                    var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
                    if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
                    {
                        model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
                        {
                            Text = template.Name,
                            Value = template.Id.ToString()
                        });
                    }
                }
            }


            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"));

            //prepare model categories
            _baseAdminModelFactory.PrepareCategories(model.AvailableCategories, false);
            foreach (var categoryItem in model.AvailableCategories)
            {
                categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                    && model.SelectedCategoryIds.Contains(categoryId);
            }

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model, product, excludeProperties);

            //prepare model stores
            _storeMappingSupportedModelFactory.PrepareModelStores(model, product, excludeProperties);

            return model;
        }

        /// <summary>
        /// Prepare paged related product list model
        /// </summary>
        /// <param name="searchModel">Related product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Related product list model</returns>
        public virtual RelatedProductListModel PrepareRelatedProductListModel(RelatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //get related products
            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId1: product.Id, showHidden: true);

            //prepare grid model
            var model = new RelatedProductListModel
            {
                Data = relatedProducts.PaginationByRequestModel(searchModel).Select(relatedProduct =>
                {
                    //fill in model values from the entity
                    var relatedProductModel = new RelatedProductModel
                    {
                        Id = relatedProduct.Id,
                        ProductId2 = relatedProduct.ProductId2,
                        DisplayOrder = relatedProduct.DisplayOrder
                    };

                    //fill in additional values (not existing in the entity)
                    relatedProductModel.Product2Name = _productService.GetProductById(relatedProduct.ProductId2)?.Name;

                    return relatedProductModel;
                }),
                Total = relatedProducts.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare related product search model to add to the product
        /// </summary>
        /// <param name="searchModel">Related product search model to add to the product</param>
        /// <returns>Related product search model to add to the product</returns>
        public virtual AddRelatedProductSearchModel PrepareAddRelatedProductSearchModel(AddRelatedProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged related product list model to add to the product
        /// </summary>
        /// <param name="searchModel">Related product search model to add to the product</param>
        /// <returns>Related product list model to add to the product</returns>
        public virtual AddRelatedProductListModel PrepareAddRelatedProductListModel(AddRelatedProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                searchModel.SearchVendorId = _workContext.CurrentVendor.Id;

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddRelatedProductListModel
            {
                //fill in model values from the entity
                Data = products.Select(product => product.ToModel<ProductModel>()),
                Total = products.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare paged associated product list model
        /// </summary>
        /// <param name="searchModel">Associated product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Associated product list model</returns>
        public virtual AssociatedProductListModel PrepareAssociatedProductListModel(AssociatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //get associated products
            var associatedProducts = _productService.GetAssociatedProducts(showHidden: true,
                parentGroupedProductId: product.Id,
                vendorId: _workContext.CurrentVendor?.Id ?? 0);

            //prepare grid model
            var model = new AssociatedProductListModel
            {
                //fill in model values from the entity
                Data = associatedProducts.PaginationByRequestModel(searchModel).Select(associatedProduct => new AssociatedProductModel
                {
                    Id = associatedProduct.Id,
                    ProductName = associatedProduct.Name,
                    DisplayOrder = associatedProduct.DisplayOrder
                }),
                Total = associatedProducts.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare associated product search model to add to the product
        /// </summary>
        /// <param name="searchModel">Associated product search model to add to the product</param>
        /// <returns>Associated product search model to add to the product</returns>
        public virtual AddAssociatedProductSearchModel PrepareAddAssociatedProductSearchModel(AddAssociatedProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged associated product list model to add to the product
        /// </summary>
        /// <param name="searchModel">Associated product search model to add to the product</param>
        /// <returns>Associated product list model to add to the product</returns>
        public virtual AddAssociatedProductListModel PrepareAddAssociatedProductListModel(AddAssociatedProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                searchModel.SearchVendorId = _workContext.CurrentVendor.Id;

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddAssociatedProductListModel
            {
                Data = products.Select(product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //fill in additional values (not existing in the entity)
                    var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                    if (parentGroupedProduct == null)
                        return productModel;

                    productModel.AssociatedToProductId = product.ParentGroupedProductId;
                    productModel.AssociatedToProductName = parentGroupedProduct.Name;

                    return productModel;
                }),
                Total = products.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare paged product picture list model
        /// </summary>
        /// <param name="searchModel">Product picture search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product picture list model</returns>
        public virtual ProductPictureListModel PrepareProductPictureListModel(ProductPictureSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //get product pictures
            var productPictures = _productService.GetProductPicturesByProductId(product.Id);

            //prepare grid model
            var model = new ProductPictureListModel
            {
                Data = productPictures.PaginationByRequestModel(searchModel).Select(productPicture =>
                {
                    //fill in model values from the entity
                    var productPictureModel = new ProductPictureModel
                    {
                        Id = productPicture.Id,
                        ProductId = productPicture.ProductId,
                        PictureId = productPicture.PictureId,
                        DisplayOrder = productPicture.DisplayOrder
                    };

                    //fill in additional values (not existing in the entity)
                    var picture = _pictureService.GetPictureById(productPicture.PictureId)
                        ?? throw new Exception("Picture cannot be loaded");

                    productPictureModel.PictureUrl = _pictureService.GetPictureUrl(picture);
                    productPictureModel.OverrideAltAttribute = picture.AltAttribute;
                    productPictureModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return productPictureModel;
                }),
                Total = productPictures.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare product tag search model
        /// </summary>
        /// <param name="searchModel">Product tag search model</param>
        /// <returns>Product tag search model</returns>
        public virtual ProductTagSearchModel PrepareProductTagSearchModel(ProductTagSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product tag list model
        /// </summary>
        /// <param name="searchModel">Product tag search model</param>
        /// <returns>Product tag list model</returns>
        public virtual ProductTagListModel PrepareProductTagListModel(ProductTagSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get product tags
            var productTags = _productTagService.GetAllProductTags()
                .OrderByDescending(tag => _productTagService.GetProductCount(tag.Id, storeId: 0)).ToList();

            //prepare list model
            var model = new ProductTagListModel
            {
                Data = productTags.PaginationByRequestModel(searchModel).Select(tag =>
                {
                    //fill in model values from the entity
                    var productTagModel = new ProductTagModel
                    {
                        Id = tag.Id,
                        Name = tag.Name
                    };

                    //fill in additional values (not existing in the entity)
                    productTagModel.ProductCount = _productTagService.GetProductCount(tag.Id, storeId: 0);

                    return productTagModel;
                }),
                Total = productTags.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare product tag model
        /// </summary>
        /// <param name="model">Product tag model</param>
        /// <param name="productTag">Product tag</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product tag model</returns>
        public virtual ProductTagModel PrepareProductTagModel(ProductTagModel model, ProductTag productTag, bool excludeProperties = false)
        {
            Action<ProductTagLocalizedModel, int> localizedModelConfiguration = null;

            if (productTag != null)
            {
                //fill in model values from the entity
                model = model ?? new ProductTagModel
                {
                    Id = productTag.Id,
                    Name = productTag.Name
                };

                model.ProductCount = _productTagService.GetProductCount(productTag.Id, storeId: 0);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(productTag, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare bulk edit product search model
        /// </summary>
        /// <param name="searchModel">Bulk edit product search model</param>
        /// <returns>Bulk edit product search model</returns>
        public virtual BulkEditProductSearchModel PrepareBulkEditProductSearchModel(BulkEditProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged bulk edit product list model
        /// </summary>
        /// <param name="searchModel">Bulk edit product search model</param>
        /// <returns>Bulk edit product list model</returns>
        public virtual BulkEditProductListModel PrepareBulkEditProductListModel(BulkEditProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                vendorId: _workContext.CurrentVendor?.Id ?? 0,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new BulkEditProductListModel
            {
                Data = products.Select(product =>
                {
                    //fill in model values from the entity
                    var productModel = new BulkEditProductModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Published = product.Published
                    };

                    return productModel;
                }),
                Total = products.TotalCount
            };

            return model;
        }
        #endregion
    }
}