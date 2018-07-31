using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly SeoSettings _seoSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public ProductModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            SeoSettings seoSettings,
            VendorSettings vendorSettings)
        {
            this._captchaSettings = captchaSettings;
            this._catalogSettings = catalogSettings;
            this._customerSettings = customerSettings;
            this._categoryService = categoryService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._downloadService = downloadService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productTagService = productTagService;
            this._productTemplateService = productTemplateService;
            this._reviewTypeService = reviewTypeService;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
            this._urlRecordService = urlRecordService;
            this._vendorService = vendorService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._mediaSettings = mediaSettings;
            this._seoSettings = seoSettings;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare the product review overview model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Product review overview model</returns>
        protected virtual ProductReviewOverviewModel PrepareProductReviewOverviewModel(Product product)
        {
            ProductReviewOverviewModel productReview;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, _storeContext.CurrentStore.Id);

                productReview = _cacheManager.Get(cacheKey, () =>
                {
                    return new ProductReviewOverviewModel
                    {
                        RatingSum = product.ProductReviews
                                .Where(pr => pr.IsApproved && pr.StoreId == _storeContext.CurrentStore.Id)
                                .Sum(pr => pr.Rating),
                        TotalReviews = product
                                .ProductReviews
                                .Count(pr => pr.IsApproved && pr.StoreId == _storeContext.CurrentStore.Id)
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel()
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }

            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
            }

            return productReview;
        }

        /// <summary>
        /// Prepare the product overview picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>Picture model</returns>
        protected virtual PictureModel PrepareProductOverviewPictureModel(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = _localizationService.GetLocalized(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY,
                product.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(),
                _storeContext.CurrentStore.Id);

            var defaultPictureModel = _cacheManager.Get(cacheKey, () =>
            {
                var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                var pictureModel = new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                    //"title" attribute
                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                        ? picture.TitleAttribute
                        : string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                        ? picture.AltAttribute
                        : string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }

        /// <summary>
        /// Prepare the product breadcrumb model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Product breadcrumb model</returns>
        protected virtual ProductDetailsModel.ProductBreadcrumbModel PrepareProductBreadcrumbModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_BREADCRUMB_MODEL_KEY,
                    product.Id,
                    _workContext.WorkingLanguage.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel
                {
                    Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                    ProductId = product.Id,
                    ProductName = _localizationService.GetLocalized(product, x => x.Name),
                    ProductSeName = _urlRecordService.GetSeName(product)
                };
                var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                if (!productCategories.Any())
                    return breadcrumbModel;

                var category = productCategories[0].Category;
                if (category == null)
                    return breadcrumbModel;

                foreach (var catBr in _categoryService.GetCategoryBreadCrumb(category))
                {
                    breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel
                    {
                        Id = catBr.Id,
                        Name = _localizationService.GetLocalized(catBr, x => x.Name),
                        SeName = _urlRecordService.GetSeName(catBr),
                        IncludeInTopMenu = catBr.IncludeInTopMenu
                    });
                }

                return breadcrumbModel;
            });
            return cachedModel;
        }

        /// <summary>
        /// Prepare the product tag models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product tag model</returns>
        protected virtual IList<ProductTagModel> PrepareProductTagModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productTagsCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(productTagsCacheKey, () =>
                _productTagService.GetAllProductTagsByProductId(product.Id)
                //filter by store
                .Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
                .Select(x => new ProductTagModel
                {
                    Id = x.Id,
                    Name = _localizationService.GetLocalized(x, y => y.Name),
                    SeName = _urlRecordService.GetSeName(x),
                    ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
                })
                .ToList());

            return model;
        }

        /// <summary>
        /// Prepare the product details picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <param name="allPictureModels">All picture models</param>
        /// <returns>Picture model for the default picture</returns>
        protected virtual PictureModel PrepareProductDetailsPictureModel(Product product, bool isAssociatedProduct, out IList<PictureModel> allPictureModels)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize, isAssociatedProduct, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
            {
                var productName = _localizationService.GetLocalized(product, x => x.Name);

                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                var defaultPicture = pictures.FirstOrDefault();
                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(defaultPicture, defaultPictureSize, !isAssociatedProduct),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(defaultPicture, 0, !isAssociatedProduct)
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName);

                //all pictures
                var pictureModels = new List<PictureModel>();
                foreach (var picture in pictures)
                {
                    var pictureModel = new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, defaultPictureSize, !isAssociatedProduct),
                        ThumbImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            allPictureModels = cachedPictures.PictureModels;
            return cachedPictures.DefaultPictureModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the product template view path
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>View path</returns>
        public virtual string PrepareProductTemplateViewPath(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
            var productTemplateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);
                if (template == null)
                    template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return productTemplateViewPath;
        }

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>Collection of product overview model</returns>
        public virtual IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePictureModel = true,
            int? productThumbPictureSize = null)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = _localizationService.GetLocalized(product, x => x.Name),
                    ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                    FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                    SeName = _urlRecordService.GetSeName(product),
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = PrepareProductOverviewPictureModel(product, productThumbPictureSize);
                }

                //reviews
                model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);

                models.Add(model);
            }
            return models;
        }

        /// <summary>
        /// Prepare the product details model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>Product details model</returns>
        public virtual ProductDetailsModel PrepareProductDetailsModel(Product product,
             bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = _localizationService.GetLocalized(product, x => x.Name),
                ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                MetaKeywords = _localizationService.GetLocalized(product, x => x.MetaKeywords),
                MetaDescription = _localizationService.GetLocalized(product, x => x.MetaDescription),
                MetaTitle = _localizationService.GetLocalized(product, x => x.MetaTitle),
                SeName = _urlRecordService.GetSeName(product),
                ProductType = product.ProductType,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //store name
            model.CurrentStoreName = _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name);

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = _localizationService.GetLocalized(vendor, x => x.Name),
                        SeName = _urlRecordService.GetSeName(vendor),
                    };
                }
            }

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }
                model.PageShareCode = shareCode;
            }

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = PrepareProductBreadcrumbModel(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = PrepareProductTagModels(product);
            }

            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            model.DefaultPictureModel = PrepareProductDetailsPictureModel(product, isAssociatedProduct, out IList<PictureModel> allPictureModels);
            model.PictureModels = allPictureModels;

           
            //product review overview
            model.ProductReviewOverview = PrepareProductReviewOverviewModel(product);

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(PrepareProductDetailsModel(associatedProduct, true));
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="model">Product reviews model</param>
        /// <param name="product">Product</param>
        /// <returns>Product reviews model</returns>
        public virtual ProductReviewsModel PrepareProductReviewsModel(ProductReviewsModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            model.ProductId = product.Id;
            model.ProductName = _localizationService.GetLocalized(product, x => x.Name);
            model.ProductSeName = _urlRecordService.GetSeName(product);

            var productReviews = _catalogSettings.ShowProductReviewsPerStore
                ? product.ProductReviews.Where(pr => pr.IsApproved && pr.StoreId == _storeContext.CurrentStore.Id)
                : product.ProductReviews.Where(pr => pr.IsApproved);

            productReviews = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                ? productReviews.OrderBy(pr => pr.CreatedOnUtc)
                : productReviews.OrderByDescending(pr => pr.CreatedOnUtc);

            //get all review types
            foreach (var reviewType in _reviewTypeService.GetAllReviewTypes())
            {
                model.ReviewTypeList.Add(new ReviewTypeModel
                {
                    Id = reviewType.Id,
                    Name = _localizationService.GetLocalized(reviewType, entity => entity.Name),
                    Description = _localizationService.GetLocalized(reviewType, entity => entity.Description),
                    VisibleToAllCustomers = reviewType.VisibleToAllCustomers,
                    DisplayOrder = reviewType.DisplayOrder,
                    IsRequired = reviewType.IsRequired,
                });
            }

            //filling data from db
            foreach (var pr in productReviews)
            {
                var customer = pr.Customer;
                var productReviewModel = new ProductReviewModel
                {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = _customerService.FormatUserName(customer),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel
                    {
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                };

                foreach (var q in _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewId(pr.Id))
                {
                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = pr.Id,
                        Rating = q.Rating,
                        Name = _localizationService.GetLocalized(q.ReviewType, x => x.Name),
                        VisibleToAllCustomers = q.ReviewType.VisibleToAllCustomers || _workContext.CurrentCustomer.Id == pr.CustomerId,
                    });
                }

                model.Items.Add(productReviewModel);
            }

            foreach (var rt in model.ReviewTypeList)
            {
                if (model.ReviewTypeList.Count <= model.AddAdditionalProductReviewList.Count) continue;
                var reviewType = _reviewTypeService.GetReviewTypeById(rt.Id);
                var reviewTypeMappingModel = new AddProductReviewReviewTypeMappingModel
                {
                    ReviewTypeId = rt.Id,
                    Name = _localizationService.GetLocalized(reviewType, entity => entity.Name),
                    Description = _localizationService.GetLocalized(reviewType, entity => entity.Description),
                    DisplayOrder = rt.DisplayOrder,
                    IsRequired = rt.IsRequired,
                };

                model.AddAdditionalProductReviewList.Add(reviewTypeMappingModel);
            }

            //Average rating
            foreach (var rtm in model.ReviewTypeList)
            {
                var totalRating = 0;
                var totalCount = 0;
                foreach (var item in model.Items)
                {
                    foreach (var q in item.AdditionalProductReviewList.Where(w => w.ReviewTypeId == rtm.Id))
                    {
                        totalRating += q.Rating;
                        totalCount = ++totalCount;
                    }
                }

                rtm.AverageRating = (double)totalRating / (totalCount > 0 ? totalCount : 1);
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_workContext.CurrentCustomer.IsGuest();
            model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;

            return model;
        }

        /// <summary>
        /// Prepare the customer product reviews model
        /// </summary>
        /// <param name="page">Number of items page; pass null to load the first page</param>
        /// <returns>Customer product reviews model</returns>
        public virtual CustomerProductReviewsModel PrepareCustomerProductReviewsModel(int? page)
        {
            var pageSize = _catalogSettings.ProductReviewsPageSizeOnAccountPage;
            var pageIndex = 0;

            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            var list = _productService.GetAllProductReviews(customerId: _workContext.CurrentCustomer.Id,
                approved: null,
                storeId: _storeContext.CurrentStore.Id,
                pageIndex: pageIndex,
                pageSize: pageSize);

            var productReviews = new List<CustomerProductReviewModel>();

            foreach (var review in list)
            {
                var product = review.Product;
                var productReviewModel = new CustomerProductReviewModel
                {
                    Title = review.Title,
                    ProductId = product.Id,
                    ProductName = _localizationService.GetLocalized(product, p => p.Name),
                    ProductSeName = _urlRecordService.GetSeName(product),
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    ReplyText = review.ReplyText,
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(review.CreatedOnUtc, DateTimeKind.Utc).ToString("g")
                };

                if (_catalogSettings.ProductReviewsMustBeApproved)
                {
                    productReviewModel.ApprovalStatus = review.IsApproved
                        ? _localizationService.GetResource("Account.CustomerProductReviews.ApprovalStatus.Approved")
                        : _localizationService.GetResource("Account.CustomerProductReviews.ApprovalStatus.Pending");
                }

                foreach (var q in _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewId(review.Id))
                {
                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = review.Id,
                        Rating = q.Rating,
                        Name = _localizationService.GetLocalized(q.ReviewType, x => x.Name),
                    });
                }

                productReviews.Add(productReviewModel);
            }

            var pagerModel = new PagerModel
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProductReviewsPaged",
                UseRouteLinks = true,
                RouteValues = new CustomerProductReviewsModel.CustomerProductReviewsRouteValues { pageNumber = pageIndex }
            };

            var model = new CustomerProductReviewsModel
            {
                ProductReviews = productReviews,
                PagerModel = pagerModel
            };

            return model;
        }

        /// <summary>
        /// Prepare the product email a friend model
        /// </summary>
        /// <param name="model">Product email a friend model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <returns>product email a friend model</returns>
        public virtual ProductEmailAFriendModel PrepareProductEmailAFriendModel(ProductEmailAFriendModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            model.ProductId = product.Id;
            model.ProductName = _localizationService.GetLocalized(product, x => x.Name);
            model.ProductSeName = _urlRecordService.GetSeName(product);
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            if (!excludeProperties)
            {
                model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            }

            return model;
        }


        #endregion
    }
}