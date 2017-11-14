﻿using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport.Help;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Stores;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ProductEditorSettings _productEditorSettings;
        private readonly IProductTemplateService _productTemplateService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
      
        #endregion

        #region Ctor
        
        public ExportManager(ICategoryService categoryService,
            ICustomerService customerService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreService storeService,
            IWorkContext workContext,
            ProductEditorSettings productEditorSettings,
            IProductTemplateService productTemplateService,
            CatalogSettings catalogSettings,
            IGenericAttributeService genericAttributeService,
            ICustomerAttributeFormatter customerAttributeFormatter)
        {
            this._categoryService = categoryService;
            this._customerService = customerService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._productEditorSettings = productEditorSettings;
            this._productTemplateService = productTemplateService;
            this._catalogSettings = catalogSettings;
            this._genericAttributeService = genericAttributeService;
            this._customerAttributeFormatter = customerAttributeFormatter;
        }

        #endregion

        #region Utilities

        protected virtual void WriteCategories(XmlWriter xmlWriter, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            if (categories == null || !categories.Any())
                return;

            foreach (var category in categories)
            {
                xmlWriter.WriteStartElement("Category");

                xmlWriter.WriteString("Id", category.Id);

                xmlWriter.WriteString("Name", category.Name);
                xmlWriter.WriteString("Description", category.Description);
                xmlWriter.WriteString("CategoryTemplateId", category.CategoryTemplateId);
                xmlWriter.WriteString("MetaKeywords", category.MetaKeywords, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("MetaDescription", category.MetaDescription, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("MetaTitle", category.MetaTitle, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("SeName", category.GetSeName(0), IgnoreExportCategoryProperty());
                xmlWriter.WriteString("ParentCategoryId", category.ParentCategoryId);
                xmlWriter.WriteString("PictureId", category.PictureId);
                xmlWriter.WriteString("PageSize", category.PageSize, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("AllowCustomersToSelectPageSize", category.AllowCustomersToSelectPageSize, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("PageSizeOptions", category.PageSizeOptions, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("ShowOnHomePage", category.ShowOnHomePage, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("IncludeInTopMenu", category.IncludeInTopMenu, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("Published", category.Published, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("Deleted", category.Deleted, true);
                xmlWriter.WriteString("DisplayOrder", category.DisplayOrder);
                xmlWriter.WriteString("CreatedOnUtc", category.CreatedOnUtc, IgnoreExportCategoryProperty());
                xmlWriter.WriteString("UpdatedOnUtc", category.UpdatedOnUtc, IgnoreExportCategoryProperty());

                xmlWriter.WriteStartElement("Products");
                var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, showHidden: true);
                foreach (var productCategory in productCategories)
                {
                    var product = productCategory.Product;
                    if (product == null || product.Deleted)
                        continue;

                    xmlWriter.WriteStartElement("ProductCategory");
                    xmlWriter.WriteString("ProductCategoryId", productCategory.Id);
                    xmlWriter.WriteString("ProductId", productCategory.ProductId);
                    xmlWriter.WriteString("ProductName", product.Name);
                    xmlWriter.WriteString("IsFeaturedProduct", productCategory.IsFeaturedProduct);
                    xmlWriter.WriteString("DisplayOrder", productCategory.DisplayOrder);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("SubCategories");
                WriteCategories(xmlWriter, category.Id);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
        }

        protected virtual void SetCaptionStyle(ExcelStyle style)
        {
            style.Fill.PatternType = ExcelFillStyle.Solid;
            style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            style.Font.Bold = true;
        }

        /// <summary>
        /// Returns the path to the image file by ID
        /// </summary>
        /// <param name="pictureId">Picture ID</param>
        /// <returns>Path to the image file</returns>
        protected virtual string GetPictures(int pictureId)
        {
            var picture = _pictureService.GetPictureById(pictureId);
            return _pictureService.GetThumbLocalPath(picture);
        }

        /// <summary>
        /// Returns the list of categories for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of categories</returns>
        protected virtual string GetCategories(Product product)
        {
            string categoryNames = null;
            foreach (var pc in _categoryService.GetProductCategoriesByProductId(product.Id, true))
            {
                categoryNames += _catalogSettings.ExportImportProductCategoryBreadcrumb ? pc.Category.GetFormattedBreadCrumb(_categoryService) : pc.Category.Name;
                categoryNames += ";";
            }

            return categoryNames;
        }

        /// <summary>
        /// Returns the list of product tag for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product tag</returns>
        protected virtual string GetProductTags(Product product)
        {
            string productTagNames = null;

            foreach (var productTag in product.ProductTags)
            {
                productTagNames += productTag.Name;
                productTagNames += ";";
            }

            return productTagNames;
        }

        /// <summary>
        /// Returns the three first image associated with the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>three first image</returns>
        protected virtual string[] GetPictures(Product product)
        {
            //pictures (up to 3 pictures)
            string picture1 = null;
            string picture2 = null;
            string picture3 = null;
            var pictures = _pictureService.GetPicturesByProductId(product.Id, 3);
            for (var i = 0; i < pictures.Count; i++)
            {
                var pictureLocalPath = _pictureService.GetThumbLocalPath(pictures[i]);
                switch (i)
                {
                    case 0:
                        picture1 = pictureLocalPath;
                        break;
                    case 1:
                        picture2 = pictureLocalPath;
                        break;
                    case 2:
                        picture3 = pictureLocalPath;
                        break;
                }
            }

            return new[] { picture1, picture2, picture3 };
        }

        /// <summary>
        /// Export objects to XLSX
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="properties">Class access to the object through its properties</param>
        /// <param name="itemsToExport">The objects to export</param>
        /// <returns></returns>
        protected virtual byte[] ExportToXlsx<T>(PropertyByName<T>[] properties, IEnumerable<T> itemsToExport)
        {
            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handles to the worksheets
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(T).Name);
                    var fWorksheet = xlPackage.Workbook.Worksheets.Add("DataForFilters");
                    fWorksheet.Hidden = eWorkSheetHidden.VeryHidden;

                    //create Headers and format them 
                    var manager = new PropertyManager<T>(properties.Where(p => !p.Ignore));
                    manager.WriteCaption(worksheet, SetCaptionStyle);

                    var row = 2;
                    foreach (var items in itemsToExport)
                    {
                        manager.CurrentObject = items;
                        manager.WriteToXlsx(worksheet, row++, _catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities, fWorksheet: fWorksheet);
                    }

                    xlPackage.Save();
                }

                return stream.ToArray();
            }
        }

        protected virtual bool IgnoreExportPoductProperty(Func<ProductEditorSettings, bool> func)
        {
            var productAdvancedMode = true;
            try
            {
                productAdvancedMode = _workContext.CurrentCustomer.GetAttribute<bool>("product-advanced-mode");
            }
            catch (ArgumentNullException)
            {
            }

            return !productAdvancedMode && !func(_productEditorSettings);
        }

        protected virtual bool IgnoreExportCategoryProperty()
        {
            try
            {
                return !_workContext.CurrentCustomer.GetAttribute<bool>("category-advanced-mode");
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        private string GetCustomCustomerAttributes(Customer customer)
        {
            var selectedCustomerAttributes = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes, _genericAttributeService);
            return _customerAttributeFormatter.FormatAttributes(selectedCustomerAttributes, ";");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export category list to XML
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCategoriesToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Categories");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
            WriteCategories(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        public virtual byte[] ExportCategoriesToXlsx(IList<Category> categories)
        {
            var parentCatagories = new List<Category>();
            if (_catalogSettings.ExportImportCategoriesUsingCategoryName)
            {
                //performance optimization, load all parent categories in one SQL request
                parentCatagories = _categoryService.GetCategoriesByIds(categories.Select(c => c.ParentCategoryId).Where(id => id != 0).ToArray());
            }

            //property array
            var properties = new[]
            {
                new PropertyByName<Category>("Id", p => p.Id),
                new PropertyByName<Category>("Name", p => p.Name),
                new PropertyByName<Category>("Description", p => p.Description),
                new PropertyByName<Category>("CategoryTemplateId", p => p.CategoryTemplateId),
                new PropertyByName<Category>("MetaKeywords", p => p.MetaKeywords, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("MetaDescription", p => p.MetaDescription, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("MetaTitle", p => p.MetaTitle, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("SeName", p => p.GetSeName(0), IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("ParentCategoryId", p => p.ParentCategoryId),
                new PropertyByName<Category>("ParentCategoryName", p => parentCatagories.FirstOrDefault(c => c.Id == p.ParentCategoryId)?.GetFormattedBreadCrumb(_categoryService), !_catalogSettings.ExportImportCategoriesUsingCategoryName),
                new PropertyByName<Category>("Picture", p => GetPictures(p.PictureId)),
                new PropertyByName<Category>("PageSize", p => p.PageSize, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("AllowCustomersToSelectPageSize", p => p.AllowCustomersToSelectPageSize, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("PageSizeOptions", p => p.PageSizeOptions, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("ShowOnHomePage", p => p.ShowOnHomePage, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("IncludeInTopMenu", p => p.IncludeInTopMenu, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("Published", p => p.Published, IgnoreExportCategoryProperty()),
                new PropertyByName<Category>("DisplayOrder", p => p.DisplayOrder)
            };
            return ExportToXlsx(properties, categories);
        }

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        public virtual byte[] ExportCustomersToXlsx(IList<Customer> customers)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<Customer>("CustomerId", p => p.Id),
                new PropertyByName<Customer>("CustomerGuid", p => p.CustomerGuid),
                new PropertyByName<Customer>("Email", p => p.Email),
                new PropertyByName<Customer>("Username", p => p.Username),
                new PropertyByName<Customer>("Password", p => _customerService.GetCurrentPassword(p.Id)?.Password),
                new PropertyByName<Customer>("PasswordFormatId", p => _customerService.GetCurrentPassword(p.Id)?.PasswordFormatId ?? 0),
                new PropertyByName<Customer>("PasswordSalt", p => _customerService.GetCurrentPassword(p.Id)?.PasswordSalt),
                new PropertyByName<Customer>("Active", p => p.Active),
                new PropertyByName<Customer>("IsGuest", p => p.IsGuest()),
                new PropertyByName<Customer>("IsRegistered", p => p.IsRegistered()),
                new PropertyByName<Customer>("IsAdministrator", p => p.IsAdmin()),
                new PropertyByName<Customer>("IsForumModerator", p => p.IsForumModerator()),
                new PropertyByName<Customer>("CreatedOnUtc", p => p.CreatedOnUtc),
                //attributes
                new PropertyByName<Customer>("FirstName", p => p.GetAttribute<string>(SystemCustomerAttributeNames.FirstName)),
                new PropertyByName<Customer>("LastName", p => p.GetAttribute<string>(SystemCustomerAttributeNames.LastName)),
                new PropertyByName<Customer>("Gender", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Gender)),
                new PropertyByName<Customer>("Company", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Company)),
                new PropertyByName<Customer>("StreetAddress", p => p.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress)),
                new PropertyByName<Customer>("StreetAddress2", p => p.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2)),
                new PropertyByName<Customer>("ZipPostalCode", p => p.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode)),
                new PropertyByName<Customer>("City", p => p.GetAttribute<string>(SystemCustomerAttributeNames.City)),
                new PropertyByName<Customer>("CountryId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)),
                new PropertyByName<Customer>("StateProvinceId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)),
                new PropertyByName<Customer>("Phone", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Phone)),
                new PropertyByName<Customer>("Fax", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Fax)),
                new PropertyByName<Customer>("TimeZoneId", p => p.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId)),
                new PropertyByName<Customer>("AvatarPictureId", p => p.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId)),
                new PropertyByName<Customer>("ForumPostCount", p => p.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount)),
                new PropertyByName<Customer>("Signature", p => p.GetAttribute<string>(SystemCustomerAttributeNames.Signature)),
                new PropertyByName<Customer>("CustomCustomerAttributes",  GetCustomCustomerAttributes)
            };

            return ExportToXlsx(properties, customers);
        }

        /// <summary>
        /// Export customer list to XML
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCustomersToXml(IList<Customer> customers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Customers");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var customer in customers)
            {
                xmlWriter.WriteStartElement("Customer");
                xmlWriter.WriteElementString("CustomerId", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);

                var customerPassword = _customerService.GetCurrentPassword(customer.Id);
                xmlWriter.WriteElementString("Password", null, customerPassword?.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, (customerPassword?.PasswordFormatId ?? 0).ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customerPassword?.PasswordSalt);

                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());

                xmlWriter.WriteElementString("IsGuest", null, customer.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, customer.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, customer.IsAdmin().ToString());
                xmlWriter.WriteElementString("IsForumModerator", null, customer.IsForumModerator().ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, customer.CreatedOnUtc.ToString());

                xmlWriter.WriteElementString("FirstName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StreetAddress", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.City));
                xmlWriter.WriteElementString("StateProvinceId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId).ToString());
                xmlWriter.WriteElementString("Phone", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax));
                xmlWriter.WriteElementString("TimeZoneId", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId));

                foreach (var store in _storeService.GetAllStores())
                {
                    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    var subscribedToNewsletters = newsletter != null && newsletter.Active;
                    xmlWriter.WriteElementString($"Newsletter-in-store-{store.Id}", null, subscribedToNewsletters.ToString());
                }

                xmlWriter.WriteElementString("AvatarPictureId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId).ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature));

                var selectedCustomerAttributesString = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes, _genericAttributeService);

                if (!string.IsNullOrEmpty(selectedCustomerAttributesString))
                {
                    var selectedCustomerAttributes = new StringReader(selectedCustomerAttributesString);
                    var selectedCustomerAttributesXmlReader = XmlReader.Create(selectedCustomerAttributes);
                    xmlWriter.WriteNode(selectedCustomerAttributesXmlReader, false);
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions)
        {
            if (subscriptions == null)
                throw new ArgumentNullException(nameof(subscriptions));

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var subscription in subscriptions)
            {
                sb.Append(subscription.Email);
                sb.Append(separator);
                sb.Append(subscription.Active);
                sb.Append(separator);
                sb.Append(subscription.StoreId);
                sb.Append(Environment.NewLine); //new line
            }

            return sb.ToString();
        }

        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportStatesToTxt(IList<StateProvince> states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var state in states)
            {
                sb.Append(state.Country.TwoLetterIsoCode);
                sb.Append(separator);
                sb.Append(state.Name);
                sb.Append(separator);
                sb.Append(state.Abbreviation);
                sb.Append(separator);
                sb.Append(state.Published);
                sb.Append(separator);
                sb.Append(state.DisplayOrder);
                sb.Append(Environment.NewLine); //new line
            }

            return sb.ToString();
        }

        #endregion
    }
}
