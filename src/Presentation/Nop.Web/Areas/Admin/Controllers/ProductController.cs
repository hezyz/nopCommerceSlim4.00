using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICategoryService _categoryService;
        private readonly ICopyProductService _copyProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INopFileProvider _fileProvider;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISettingService _settingService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public ProductController(IAclService aclService,
            ICategoryService categoryService,
            IProductModelFactory productModelFactory,
             ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDownloadService downloadService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INopFileProvider fileProvider,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            VendorSettings vendorSettings)
        {
            this._aclService = aclService;
            this._categoryService = categoryService;
            this._productModelFactory = productModelFactory;
            this._copyProductService = copyProductService;
            this._customerActivityService = customerActivityService;
            this._customerService = customerService;
            this._downloadService = downloadService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._fileProvider = fileProvider;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productTagService = productTagService;
            this._settingService = settingService;
            this._storeMappingService = storeMappingService;
            this._storeService = storeService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(product, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(product, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productTag,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                var seName = _urlRecordService.ValidateSeName(productTag, string.Empty, localized.Name, false);
                _urlRecordService.SaveSlug(productTag, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }

        protected virtual void SaveProductAcl(Product product, ProductModel model)
        {
            product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();

            var existingAclRecords = _aclService.GetAclRecords(product);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(product, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        protected virtual void SaveStoreMappings(Product product, ProductModel model)
        {
            product.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = _storeMappingService.GetStoreMappings(product);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(product, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        protected virtual void SaveCategoryMappings(Product product, ProductModel model)
        {
            var existingProductCategories = _categoryService.GetProductCategoriesByProductId(product.Id, true);

            //delete categories
            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    _categoryService.DeleteProductCategory(existingProductCategory);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    _categoryService.InsertProductCategory(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(productTags))
                return result.ToArray();

            var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        #endregion

        #region Methods

        #region Product list / create / edit / delete

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareProductSearchModel(new ProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ProductList(ProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareProductListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 && _workContext.CurrentVendor != null
                && _productService.GetNumberOfProductsByVendorId(_workContext.CurrentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            //prepare model
            var model = _productModelFactory.PrepareProductModel(new ProductModel(),null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 && _workContext.CurrentVendor != null
                && _productService.GetNumberOfProductsByVendorId(_workContext.CurrentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                    model.VendorId = _workContext.CurrentVendor.Id;

                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage)
                    model.ShowOnHomePage = false;

                //product
                var product = model.ToEntity<Product>();
                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.InsertProduct(product);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(product, model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);

                //locales
                UpdateLocales(product, model);

                //categories
                SaveCategoryMappings(product, model);

                //ACL (customer roles)
                SaveProductAcl(product, model);

                //stores
                SaveStoreMappings(product, model);

                //tags
                _productTagService.UpdateProductTags(product, ParseProductTags(model.ProductTags));

                //activity log
                _customerActivityService.InsertActivity("AddNewProduct",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name), product);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = _productModelFactory.PrepareProductModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            //prepare model
            var model = _productModelFactory.PrepareProductModel(null, product);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");


            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                    model.VendorId = _workContext.CurrentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage != product.ShowOnHomePage)
                    model.ShowOnHomePage = product.ShowOnHomePage;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(product, model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);

                //locales
                UpdateLocales(product, model);

                //tags
                _productTagService.UpdateProductTags(product, ParseProductTags(model.ProductTags));

                //categories
                SaveCategoryMappings(product, model);

                //ACL (customer roles)
                SaveProductAcl(product, model);

                //stores
                SaveStoreMappings(product, model);

                //picture seo names
                UpdatePictureSeoNames(product);

                //activity log
                _customerActivityService.InsertActivity("EditProduct",
                    string.Format(_localizationService.GetResource("ActivityLog.EditProduct"), product.Name), product);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = _productModelFactory.PrepareProductModel(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = _productService.GetProductById(id);
            if (product == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            _productService.DeleteProduct(product);

            //activity log
            _customerActivityService.InsertActivity("DeleteProduct",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name), product);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual IActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                _productService.DeleteProducts(_productService.GetProductsByIds(selectedIds.ToArray()).Where(p => _workContext.CurrentVendor == null || p.VendorId == _workContext.CurrentVendor.Id).ToList());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult CopyProduct(ProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List");

                var newProduct = _copyProductService.CopyProduct(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyImages);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Copied"));

                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Related products

        [HttpPost]
        public virtual IActionResult RelatedProductList(RelatedProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //try to get a product with the specified id
            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = _productModelFactory.PrepareRelatedProductListModel(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult RelatedProductUpdate(RelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a related product with the specified id
            var relatedProduct = _productService.GetRelatedProductById(model.Id)
                ?? throw new ArgumentException("No related product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(relatedProduct.ProductId1);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    return Content("This is not your product");
            }

            relatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult RelatedProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a related product with the specified id
            var relatedProduct = _productService.GetRelatedProductById(id)
                ?? throw new ArgumentException("No related product found with the specified id");

            var productId = relatedProduct.ProductId1;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    return Content("This is not your product");
            }

            _productService.DeleteRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        public virtual IActionResult RelatedProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareAddRelatedProductSearchModel(new AddRelatedProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult RelatedProductAddPopupList(AddRelatedProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareAddRelatedProductListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult RelatedProductAddPopup(AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingRelatedProducts = _productService.GetRelatedProductsByProductId1(model.ProductId);
                foreach (var product in selectedProducts)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    if (_productService.FindRelatedProduct(existingRelatedProducts, model.ProductId, product.Id) != null)
                        continue;

                    _productService.InsertRelatedProduct(new RelatedProduct
                    {
                        ProductId1 = model.ProductId,
                        ProductId2 = product.Id,
                        DisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddRelatedProductSearchModel());
        }

        #endregion

        #region Associated products

        [HttpPost]
        public virtual IActionResult AssociatedProductList(AssociatedProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //try to get a product with the specified id
            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = _productModelFactory.PrepareAssociatedProductListModel(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult AssociatedProductUpdate(AssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get an associated product with the specified id
            var associatedProduct = _productService.GetProductById(model.Id)
                ?? throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            associatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProduct(associatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult AssociatedProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get an associated product with the specified id
            var product = _productService.GetProductById(id)
                ?? throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            product.ParentGroupedProductId = 0;
            _productService.UpdateProduct(product);

            return new NullJsonResult();
        }

        public virtual IActionResult AssociatedProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareAddAssociatedProductSearchModel(new AddAssociatedProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AssociatedProductAddPopupList(AddAssociatedProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareAddAssociatedProductListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult AssociatedProductAddPopup(AddAssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    product.ParentGroupedProductId = model.ProductId;
                    _productService.UpdateProduct(product);
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddAssociatedProductSearchModel());
        }

        #endregion

        #region Product pictures

        public virtual IActionResult ProductPictureAdd(int pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = _productService.GetProductById(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (_productService.GetProductPicturesByProductId(productId).Any(p => p.PictureId == pictureId))
                return Json(new { Result = false });

            //try to get a picture with the specified id
            var picture = _pictureService.GetPictureById(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));

            _productService.InsertProductPicture(new ProductPicture
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder
            });

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult ProductPictureList(ProductPictureSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //try to get a product with the specified id
            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = _productModelFactory.PrepareProductPictureListModel(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ProductPictureUpdate(ProductPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = _productService.GetProductPictureById(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productPicture.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    return Content("This is not your product");
            }

            //try to get a picture with the specified id
            var picture = _pictureService.GetPictureById(productPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            productPicture.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProductPicture(productPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ProductPictureDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = _productService.GetProductPictureById(id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productPicture.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    return Content("This is not your product");
            }

            var pictureId = productPicture.PictureId;
            _productService.DeleteProductPicture(productPicture);

            //try to get a picture with the specified id
            var picture = _pictureService.GetPictureById(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            _pictureService.DeletePicture(picture);

            return new NullJsonResult();
        }

        #endregion

        #region Product tags

        public virtual IActionResult ProductTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareProductTagSearchModel(new ProductTagSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ProductTags(ProductTagSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareProductTagListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ProductTagDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var tag = _productTagService.GetProductTagById(id)
                ?? throw new ArgumentException("No product tag found with the specified id");

            _productTagService.DeleteProductTag(tag);

            return new NullJsonResult();
        }

        public virtual IActionResult EditProductTag(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var productTag = _productTagService.GetProductTagById(id);
            if (productTag == null)
                return RedirectToAction("List");

            //prepare tag model
            var model = _productModelFactory.PrepareProductTagModel(null, productTag);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult EditProductTag(ProductTagModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var productTag = _productTagService.GetProductTagById(model.Id);
            if (productTag == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                _productTagService.UpdateProductTag(productTag);

                //locales
                UpdateLocales(productTag, model);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //prepare model
            model = _productModelFactory.PrepareProductTagModel(model, productTag, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Bulk editing

        public virtual IActionResult BulkEdit()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareBulkEditProductSearchModel(new BulkEditProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult BulkEditSelect(BulkEditProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareBulkEditProductListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult BulkEditUpdate(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products == null)
                return new NullJsonResult();

            foreach (var pModel in products)
            {
                //update
                var product = _productService.GetProductById(pModel.Id);
                if (product == null)
                    continue;

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                    continue;


                product.Name = pModel.Name;
                product.Published = pModel.Published;
                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);

            }

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult BulkEditDelete(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products == null)
                return new NullJsonResult();

            foreach (var pModel in products)
            {
                //delete
                var product = _productService.GetProductById(pModel.Id);
                if (product == null)
                    continue;

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                    continue;

                _productService.DeleteProduct(product);
            }

            return new NullJsonResult();
        }

        #endregion

        #region Product editor settings

        [HttpPost]
        public virtual IActionResult SaveProductEditorSettings(ProductModel model, string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //vendors cannot manage these settings
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            var productEditorSettings = _settingService.LoadSetting<ProductEditorSettings>();
            productEditorSettings = model.ProductEditorSettingsModel.ToSettings(productEditorSettings);
            _settingService.SaveSetting(productEditorSettings);

            //product list
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("List");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("List");

            return Redirect(returnUrl);
        }

        #endregion

        #endregion
    }
}