using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IAclService _aclService;
        private readonly ICacheManager _cacheManager;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly string _entityName;

        #endregion

        #region Ctor

        public ProductService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICacheManager cacheManager,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IRepository<AclRecord> aclRepository,
            IRepository<Product> productRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings)
        {
            this._catalogSettings = catalogSettings;
            this._commonSettings = commonSettings;
            this._aclService = aclService;
            this._cacheManager = cacheManager;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._aclRepository = aclRepository;
            this._productRepository = productRepository;
            this._productPictureRepository = productPictureRepository;
            this._productReviewRepository = productReviewRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._storeMappingService = storeMappingService;
            this._workContext = workContext;
            this._localizationSettings = localizationSettings;
            this._entityName = typeof(Product).Name;
        }

        #endregion

        #region Methods

        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Deleted = true;
            //delete product
            UpdateProduct(product);

            //event notification
            _eventPublisher.EntityDeleted(product);
        }

        /// <summary>
        /// Delete products
        /// </summary>
        /// <param name="products">Products</param>
        public virtual void DeleteProducts(IList<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            foreach (var product in products)
            {
                product.Deleted = true;
            }

            //delete product
            UpdateProducts(products);

            foreach (var product in products)
            {
                //event notification
                _eventPublisher.EntityDeleted(product);
            }
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        public virtual IList<Product> GetAllProductsDisplayedOnHomePage()
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowOnHomePage
                        select p;
            var products = query.ToList();
            return products;
        }

        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public virtual Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            var key = string.Format(NopCatalogDefaults.ProductsByIdCacheKey, productId);
            return _cacheManager.Get(key, () => _productRepository.GetById(productId));
        }

        /// <summary>
        /// Get products by identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetProductsByIds(int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var query = from p in _productRepository.Table
                        where productIds.Contains(p.Id) && !p.Deleted
                        select p;
            var products = query.ToList();
            //sort by passed identifiers
            var sortedProducts = new List<Product>();
            foreach (var id in productIds)
            {
                var product = products.Find(x => x.Id == id);
                if (product != null)
                    sortedProducts.Add(product);
            }

            return sortedProducts;
        }

        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //insert
            _productRepository.Insert(product);

            //clear cache
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductsPatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(product);
        }

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //update
            _productRepository.Update(product);

            //cache
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductsPatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(product);
        }

        /// <summary>
        /// Update products
        /// </summary>
        /// <param name="products">Products</param>
        public virtual void UpdateProducts(IList<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            //update
            _productRepository.Update(products);

            //cache
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductsPatternCacheKey);

            //event notification
            foreach (var product in products)
            {
                _eventPublisher.EntityUpdated(product);
            }
        }

        /// <summary>
        /// Get number of product (published and visible) in certain category
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Number of products</returns>
        public virtual int GetNumberOfProductsInCategory(IList<int> categoryIds = null, int storeId = 0)
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted && p.Published && p.VisibleIndividually);

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                        select p;
            }

            if (!_catalogSettings.IgnoreAcl)
            {
                //Access control list. Allowed customer roles
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();

                query = from p in query
                        join acl in _aclRepository.Table
                        on new { c1 = p.Id, c2 = _entityName } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                        from acl in p_acl.DefaultIfEmpty()
                        where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                        select p;
            }

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = from p in query
                        join sm in _storeMappingRepository.Table
                        on new { c1 = p.Id, c2 = _entityName } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                        from sm in p_sm.DefaultIfEmpty()
                        where !p.LimitedToStores || storeId == sm.StoreId
                        select p;
            }

            //only distinct products
            var result = query.Select(p => p.Id).Distinct().Count();
            return result;
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        public virtual IPagedList<Product> SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int storeId = 0,
            int vendorId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchProductTags = false,
            int languageId = 0,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            //search by keyword
            var searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages().Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();

            //pass category identifiers as comma-delimited string
            var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

            //pass customer role identifiers as comma-delimited string
            var commaSeparatedAllowedCustomerRoleIds = string.Join(",", allowedCustomerRolesIds);

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare input parameters
            var pCategoryIds = _dataProvider.GetStringParameter("CategoryIds", commaSeparatedCategoryIds);
            var pStoreId = _dataProvider.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
            var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);
            var pProductTypeId = _dataProvider.GetInt32Parameter("ProductTypeId", (int?)productType);
            var pVisibleIndividuallyOnly = _dataProvider.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
            var pMarkedAsNewOnly = _dataProvider.GetBooleanParameter("MarkedAsNewOnly", markedAsNewOnly);
            var pProductTagId = _dataProvider.GetInt32Parameter("ProductTagId", productTagId);
            var pFeaturedProducts = _dataProvider.GetBooleanParameter("FeaturedProducts", featuredProducts);
            var pKeywords = _dataProvider.GetStringParameter("Keywords", keywords);
            var pSearchDescriptions = _dataProvider.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            var pSearchProductTags = _dataProvider.GetBooleanParameter("SearchProductTags", searchProductTags);
            var pUseFullTextSearch = _dataProvider.GetBooleanParameter("UseFullTextSearch", _commonSettings.UseFullTextSearch);
            var pFullTextMode = _dataProvider.GetInt32Parameter("FullTextMode", (int)_commonSettings.FullTextMode);
            var pLanguageId = _dataProvider.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
            var pOrderBy = _dataProvider.GetInt32Parameter("OrderBy", (int)orderBy);
            var pAllowedCustomerRoleIds = _dataProvider.GetStringParameter("AllowedCustomerRoleIds", !_catalogSettings.IgnoreAcl ? commaSeparatedAllowedCustomerRoleIds : string.Empty);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
            var pShowHidden = _dataProvider.GetBooleanParameter("ShowHidden", showHidden);
            var pOverridePublished = _dataProvider.GetBooleanParameter("OverridePublished", overridePublished);
          
            //prepare output parameters
            var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");

            //invoke stored procedure
            var products = _dbContext.EntityFromSql<Product>("ProductLoadAllPaged",
                pCategoryIds,
                pStoreId,
                pVendorId,
                pProductTypeId,
                pVisibleIndividuallyOnly,
                pMarkedAsNewOnly,
                pProductTagId,
                pFeaturedProducts,
                pKeywords,
                pSearchDescriptions,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pLanguageId,
                pOrderBy,
                pAllowedCustomerRoleIds,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pOverridePublished,
                pTotalRecords).ToList();

            //return products
            var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);
        }


        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetAssociatedProducts(int parentGroupedProductId,
            int storeId = 0,int vendorId = 0, bool showHidden = false)
        {
            var query = _productRepository.Table;
            query = query.Where(x => x.ParentGroupedProductId == parentGroupedProductId);
            if (!showHidden)
            {
                query = query.Where(x => x.Published);
            }

            //vendor filtering
            if (vendorId > 0)
            {
                query = query.Where(p => p.VendorId == vendorId);
            }

            query = query.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

            var products = query.ToList();

            //ACL mapping
            if (!showHidden)
            {
                products = products.Where(x => _aclService.Authorize(x)).ToList();
            }

            //Store mapping
            if (!showHidden && storeId > 0)
            {
                products = products.Where(x => _storeMappingService.Authorize(x, storeId)).ToList();
            }

            return products;
        }

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProductReviewTotals(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var approvedRatingSum = 0;
            var notApprovedRatingSum = 0;
            var approvedTotalReviews = 0;
            var notApprovedTotalReviews = 0;
            var reviews = product.ProductReviews;
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            product.ApprovedRatingSum = approvedRatingSum;
            product.NotApprovedRatingSum = notApprovedRatingSum;
            product.ApprovedTotalReviews = approvedTotalReviews;
            product.NotApprovedTotalReviews = notApprovedTotalReviews;
            UpdateProduct(product);
        }

        /// <summary>
        /// Gets number of products by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Number of products</returns>
        public int GetNumberOfProductsByVendorId(int vendorId)
        {
            if (vendorId == 0)
                return 0;

            return _productRepository.Table.Count(p => p.VendorId == vendorId && !p.Deleted);
        }

        /// <summary>
        /// Indicates whether a product tag exists
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Result</returns>
        public virtual bool ProductTagExists(Product product, int productTagId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var result = product.ProductProductTagMappings.Any(pt => pt.ProductTagId == productTagId);
            return result;
        }


        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException(nameof(relatedProduct));

            _relatedProductRepository.Delete(relatedProduct);

            //event notification
            _eventPublisher.EntityDeleted(relatedProduct);
        }

        /// <summary>
        /// Gets related products by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Related products</returns>
        public virtual IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from rp in _relatedProductRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId1 &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby rp.DisplayOrder, rp.Id
                        select rp;
            var relatedProducts = query.ToList();

            return relatedProducts;
        }

        /// <summary>
        /// Gets a related product
        /// </summary>
        /// <param name="relatedProductId">Related product identifier</param>
        /// <returns>Related product</returns>
        public virtual RelatedProduct GetRelatedProductById(int relatedProductId)
        {
            if (relatedProductId == 0)
                return null;

            return _relatedProductRepository.GetById(relatedProductId);
        }

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException(nameof(relatedProduct));

            _relatedProductRepository.Insert(relatedProduct);

            //event notification
            _eventPublisher.EntityInserted(relatedProduct);
        }

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException(nameof(relatedProduct));

            _relatedProductRepository.Update(relatedProduct);

            //event notification
            _eventPublisher.EntityUpdated(relatedProduct);
        }

        /// <summary>
        /// Finds a related product item by specified identifiers
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="productId2">The second product identifier</param>
        /// <returns>Related product</returns>
        public virtual RelatedProduct FindRelatedProduct(IList<RelatedProduct> source, int productId1, int productId2)
        {
            foreach (var relatedProduct in source)
                if (relatedProduct.ProductId1 == productId1 && relatedProduct.ProductId2 == productId2)
                    return relatedProduct;
            return null;
        }

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void DeleteProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException(nameof(productPicture));

            _productPictureRepository.Delete(productPicture);

            //event notification
            _eventPublisher.EntityDeleted(productPicture);
        }

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Product pictures</returns>
        public virtual IList<ProductPicture> GetProductPicturesByProductId(int productId)
        {
            var query = from pp in _productPictureRepository.Table
                        where pp.ProductId == productId
                        orderby pp.DisplayOrder, pp.Id
                        select pp;
            var productPictures = query.ToList();
            return productPictures;
        }

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>Product picture</returns>
        public virtual ProductPicture GetProductPictureById(int productPictureId)
        {
            if (productPictureId == 0)
                return null;

            return _productPictureRepository.GetById(productPictureId);
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void InsertProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException(nameof(productPicture));

            _productPictureRepository.Insert(productPicture);

            //event notification
            _eventPublisher.EntityInserted(productPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void UpdateProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException(nameof(productPicture));

            _productPictureRepository.Update(productPicture);

            //event notification
            _eventPublisher.EntityUpdated(productPicture);
        }

        /// <summary>
        /// Get the IDs of all product images 
        /// </summary>
        /// <param name="productsIds">Products IDs</param>
        /// <returns>All picture identifiers grouped by product ID</returns>
        public IDictionary<int, int[]> GetProductsImagesIds(int[] productsIds)
        {
            var productPictures = _productPictureRepository.Table.Where(p => productsIds.Contains(p.ProductId)).ToList();

            return productPictures.GroupBy(p => p.ProductId).ToDictionary(p => p.Key, p => p.Select(p1 => p1.PictureId).ToArray());
        }

        #endregion

        #region Product reviews

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier (who wrote a review); 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="productId">The product identifier; pass 0 to load all records</param>
        /// <param name="vendorId">The vendor identifier (limit to products of this vendor); pass 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Reviews</returns>
        public virtual IPagedList<ProductReview> GetAllProductReviews(int customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productReviewRepository.Table;
            if (approved.HasValue)
                query = query.Where(pr => pr.IsApproved == approved);
            if (customerId > 0)
                query = query.Where(pr => pr.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(pr => fromUtc.Value <= pr.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(pr => toUtc.Value >= pr.CreatedOnUtc);
            if (!string.IsNullOrEmpty(message))
                query = query.Where(pr => pr.Title.Contains(message) || pr.ReviewText.Contains(message));
            if (storeId > 0 && (showHidden || _catalogSettings.ShowProductReviewsPerStore))
                query = query.Where(pr => pr.StoreId == storeId);
            if (productId > 0)
                query = query.Where(pr => pr.ProductId == productId);
            if (vendorId > 0)
                query = query.Where(pr => pr.Product.VendorId == vendorId);

            //ignore deleted products
            query = query.Where(pr => !pr.Product.Deleted);

            //filter by limited to store products
            if (storeId > 0 && !showHidden && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = from productReview in query
                        join storeMapping in _storeMappingRepository.Table
                            on new { Id = productReview.ProductId, Name = nameof(Product) }
                            equals new { Id = storeMapping.EntityId, Name = storeMapping.EntityName } into storeMappingsWithNulls
                        from storeMapping in storeMappingsWithNulls.DefaultIfEmpty()
                        where !productReview.Product.LimitedToStores || storeMapping.StoreId == storeId
                        select productReview;
            }

            query = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                ? query.OrderBy(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id)
                : query.OrderByDescending(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id);

            var productReviews = new PagedList<ProductReview>(query, pageIndex, pageSize);

            return productReviews;
        }

        /// <summary>
        /// Gets product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <returns>Product review</returns>
        public virtual ProductReview GetProductReviewById(int productReviewId)
        {
            if (productReviewId == 0)
                return null;

            return _productReviewRepository.GetById(productReviewId);
        }

        /// <summary>
        /// Get product reviews by identifiers
        /// </summary>
        /// <param name="productReviewIds">Product review identifiers</param>
        /// <returns>Product reviews</returns>
        public virtual IList<ProductReview> GetProducReviewsByIds(int[] productReviewIds)
        {
            if (productReviewIds == null || productReviewIds.Length == 0)
                return new List<ProductReview>();

            var query = from pr in _productReviewRepository.Table
                        where productReviewIds.Contains(pr.Id)
                        select pr;
            var productReviews = query.ToList();
            //sort by passed identifiers
            var sortedProductReviews = new List<ProductReview>();
            foreach (var id in productReviewIds)
            {
                var productReview = productReviews.Find(x => x.Id == id);
                if (productReview != null)
                    sortedProductReviews.Add(productReview);
            }

            return sortedProductReviews;
        }

        /// <summary>
        /// Deletes a product review
        /// </summary>
        /// <param name="productReview">Product review</param>
        public virtual void DeleteProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException(nameof(productReview));

            _productReviewRepository.Delete(productReview);

            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductsPatternCacheKey);
            //event notification
            _eventPublisher.EntityDeleted(productReview);
        }

        /// <summary>
        /// Deletes product reviews
        /// </summary>
        /// <param name="productReviews">Product reviews</param>
        public virtual void DeleteProductReviews(IList<ProductReview> productReviews)
        {
            if (productReviews == null)
                throw new ArgumentNullException(nameof(productReviews));

            _productReviewRepository.Delete(productReviews);

            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductsPatternCacheKey);
            //event notification
            foreach (var productReview in productReviews)
            {
                _eventPublisher.EntityDeleted(productReview);
            }
        }

        #endregion

        #endregion
    }
}