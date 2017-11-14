using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        #region Fields

        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA1";

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IEncryptionService _encryptionService;
        private readonly IDataProvider _dataProvider;
        private readonly MediaSettings _mediaSettings;
        private readonly IProductTemplateService _productTemplateService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductTagService _productTagService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public ImportManager(IProductService productService,
            ICategoryService categoryService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IEncryptionService encryptionService,
            IDataProvider dataProvider,
            MediaSettings mediaSettings,
            IProductTemplateService productTemplateService,
            CatalogSettings catalogSettings,
            IProductTagService productTagService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._pictureService = pictureService;
            this._urlRecordService = urlRecordService;
            this._storeContext = storeContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._encryptionService = encryptionService;
            this._dataProvider = dataProvider;
            this._mediaSettings = mediaSettings;
            this._productTemplateService = productTemplateService;
            this._catalogSettings = catalogSettings;
            this._productTagService = productTagService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
        }

        #endregion

        #region Utilities

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            for (var i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }

        protected virtual string ConvertColumnToString(object columnValue)
        {
            if (columnValue == null)
                return null;

            return Convert.ToString(columnValue);
        }

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            //TODO test ne implementation
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out string mimeType);
            //set to jpeg in case mime type cannot be found
            if (mimeType == null)
                mimeType = MimeTypes.ImageJpeg;
            return mimeType;
        }

        /// <summary>
        /// Creates or loads the image
        /// </summary>
        /// <param name="picturePath">The path to the image file</param>
        /// <param name="name">The name of the object</param>
        /// <param name="picId">Image identifier, may be null</param>
        /// <returns>The image or null if the image has not changed</returns>
        protected virtual Picture LoadPicture(string picturePath, string name, int? picId = null)
        {
            if (string.IsNullOrEmpty(picturePath) || !File.Exists(picturePath))
                return null;

            var mimeType = GetMimeTypeFromFilePath(picturePath);
            var newPictureBinary = File.ReadAllBytes(picturePath);
            var pictureAlreadyExists = false;
            if (picId != null)
            {
                //compare with existing product pictures
                var existingPicture = _pictureService.GetPictureById(picId.Value);
                if (existingPicture != null)
                {
                    var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                    //picture binary after validation (like in database)
                    var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                    if (existingBinary.SequenceEqual(validatedPictureBinary) ||
                        existingBinary.SequenceEqual(newPictureBinary))
                    {
                        pictureAlreadyExists = true;
                    }
                }
            }

            if (pictureAlreadyExists) return null;

            var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(name));
            return newPicture;
        }

        protected virtual string UpdateCategoryByXlsx(Category category, PropertyManager<Category> manager, Dictionary<string, Category> allCategories, bool isNew, out bool isParentCategoryExists)
        {
            var seName = string.Empty;
            isParentCategoryExists = true;
            var isParentCategorySet = false;

            foreach (var property in manager.GetProperties)
            {
                switch (property.PropertyName)
                {
                    case "Name":
                        category.Name = property.StringValue.Split(new[] { ">>" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
                        break;
                    case "Description":
                        category.Description = property.StringValue;
                        break;
                    case "CategoryTemplateId":
                        category.CategoryTemplateId = property.IntValue;
                        break;
                    case "MetaKeywords":
                        category.MetaKeywords = property.StringValue;
                        break;
                    case "MetaDescription":
                        category.MetaDescription = property.StringValue;
                        break;
                    case "MetaTitle":
                        category.MetaTitle = property.StringValue;
                        break;
                    case "ParentCategoryId":
                        if (!isParentCategorySet)
                        {
                            var parentCategory = allCategories.Values.FirstOrDefault(c => c.Id == property.IntValue);
                            isParentCategorySet = parentCategory != null;

                            isParentCategoryExists = isParentCategorySet || property.IntValue == 0;

                            category.ParentCategoryId = parentCategory?.Id ?? property.IntValue;
                        }
                        break;
                    case "ParentCategoryName":
                        if (_catalogSettings.ExportImportCategoriesUsingCategoryName && !isParentCategorySet)
                        {
                            var categoryName = manager.GetProperty("ParentCategoryName").StringValue;
                            if (!string.IsNullOrEmpty(categoryName))
                            {
                                var parentCategory = allCategories.ContainsKey(categoryName)
                                    //try find category by full name with all parent category names
                                    ? allCategories[categoryName]
                                    //try find category by name
                                    : allCategories.Values.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.InvariantCulture));

                                if (parentCategory != null)
                                {
                                    category.ParentCategoryId = parentCategory.Id;
                                    isParentCategorySet = true;
                                }
                                else
                                {
                                    isParentCategoryExists = false;
                                }
                            }
                        }
                        break;
                    case "Picture":
                        var picture = LoadPicture(manager.GetProperty("Picture").StringValue, category.Name, isNew ? null : (int?)category.PictureId);
                        if (picture != null)
                            category.PictureId = picture.Id;
                        break;
                    case "PageSize":
                        category.PageSize = property.IntValue;
                        break;
                    case "AllowCustomersToSelectPageSize":
                        category.AllowCustomersToSelectPageSize = property.BooleanValue;
                        break;
                    case "PageSizeOptions":
                        category.PageSizeOptions = property.StringValue;
                        break;
                    case "ShowOnHomePage":
                        category.ShowOnHomePage = property.BooleanValue;
                        break;
                    case "IncludeInTopMenu":
                        category.IncludeInTopMenu = property.BooleanValue;
                        break;
                    case "Published":
                        category.Published = property.BooleanValue;
                        break;
                    case "DisplayOrder":
                        category.DisplayOrder = property.IntValue;
                        break;
                    case "SeName":
                        seName = property.StringValue;
                        break;
                }
            }

            category.UpdatedOnUtc = DateTime.UtcNow;
            return seName;
        }

        protected virtual Category GetCategoryFromXlsx(PropertyManager<Category> manager, ExcelWorksheet worksheet, int iRow, Dictionary<string, Category> allCategories, out bool isNew, out string curentCategoryBreadCrumb)
        {
            manager.ReadFromXlsx(worksheet, iRow);

            //try get category from database by ID
            var category = allCategories.Values.FirstOrDefault(c => c.Id == manager.GetProperty("Id")?.IntValue);

            if (_catalogSettings.ExportImportCategoriesUsingCategoryName && category == null)
            {
                var categoryName = manager.GetProperty("Name").StringValue;
                if (!string.IsNullOrEmpty(categoryName))
                {
                    category = allCategories.ContainsKey(categoryName)
                        //try find category by full name with all parent category names
                        ? allCategories[categoryName]
                        //try find category by name
                        : allCategories.Values.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.InvariantCulture));
                }
            }

            isNew = category == null;

            category = category ?? new Category();

            curentCategoryBreadCrumb = string.Empty;

            if (isNew)
            {
                category.CreatedOnUtc = DateTime.UtcNow;
                //default values
                category.PageSize = _catalogSettings.DefaultCategoryPageSize;
                category.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
                category.Published = true;
                category.IncludeInTopMenu = true;
                category.AllowCustomersToSelectPageSize = true;
            }
            else
            {
                curentCategoryBreadCrumb = category.GetFormattedBreadCrumb(_categoryService);
            }

            return category;
        }

        protected virtual void SaveCategory(bool isNew, Category category, Dictionary<string, Category> allCategories, string curentCategoryBreadCrumb, bool setSeName, string seName)
        {
            if (isNew)
                _categoryService.InsertCategory(category);
            else
                _categoryService.UpdateCategory(category);

            var categoryBreadCrumb = category.GetFormattedBreadCrumb(_categoryService);
            if (!allCategories.ContainsKey(categoryBreadCrumb))
                allCategories.Add(categoryBreadCrumb, category);
            if (!string.IsNullOrEmpty(curentCategoryBreadCrumb) && allCategories.ContainsKey(curentCategoryBreadCrumb) &&
                categoryBreadCrumb != curentCategoryBreadCrumb)
                allCategories.Remove(curentCategoryBreadCrumb);

            //search engine name
            if (setSeName)
                _urlRecordService.SaveSlug(category, category.ValidateSeName(seName, category.Name, true), 0);
        }     
        #endregion

        #region Methods

        /// <summary>
        /// Get property list by excel cells
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="worksheet">Excel worksheet</param>
        /// <returns>Property list</returns>
        public static IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(ExcelWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Cells[1, poz];

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }

        /// <summary>
        /// Import newsletter subscribers from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported subscribers</returns>
        public virtual int ImportNewsletterSubscribersFromTxt(Stream stream)
        {
            var count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var tmp = line.Split(',');

                    string email;
                    var isActive = true;
                    var storeId = _storeContext.CurrentStore.Id;
                    //parse
                    if (tmp.Length == 1)
                    {
                        //"email" only
                        email = tmp[0].Trim();
                    }
                    else if (tmp.Length == 2)
                    {
                        //"email" and "active" fields specified
                        email = tmp[0].Trim();
                        isActive = bool.Parse(tmp[1].Trim());
                    }
                    else if (tmp.Length == 3)
                    {
                        //"email" and "active" and "storeId" fields specified
                        email = tmp[0].Trim();
                        isActive = bool.Parse(tmp[1].Trim());
                        storeId = int.Parse(tmp[2].Trim());
                    }
                    else
                        throw new NopException("Wrong file format");

                    //import
                    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, storeId);
                    if (subscription != null)
                    {
                        subscription.Email = email;
                        subscription.Active = isActive;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
                    }
                    else
                    {
                        subscription = new NewsLetterSubscription
                        {
                            Active = isActive,
                            CreatedOnUtc = DateTime.UtcNow,
                            Email = email,
                            StoreId = storeId,
                            NewsLetterSubscriptionGuid = Guid.NewGuid()
                        };
                        _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    }

                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import states from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported states</returns>
        public virtual int ImportStatesFromTxt(Stream stream)
        {
            var count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var tmp = line.Split(',');

                    if (tmp.Length != 5)
                        throw new NopException("Wrong file format");

                    //parse
                    var countryTwoLetterIsoCode = tmp[0].Trim();
                    var name = tmp[1].Trim();
                    var abbreviation = tmp[2].Trim();
                    var published = bool.Parse(tmp[3].Trim());
                    var displayOrder = int.Parse(tmp[4].Trim());

                    var country = _countryService.GetCountryByTwoLetterIsoCode(countryTwoLetterIsoCode);
                    if (country == null)
                    {
                        //country cannot be loaded. skip
                        continue;
                    }

                    //import
                    var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true);
                    var state = states.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                    if (state != null)
                    {
                        state.Abbreviation = abbreviation;
                        state.Published = published;
                        state.DisplayOrder = displayOrder;
                        _stateProvinceService.UpdateStateProvince(state);
                    }
                    else
                    {
                        state = new StateProvince
                        {
                            CountryId = country.Id,
                            Name = name,
                            Abbreviation = abbreviation,
                            Published = published,
                            DisplayOrder = displayOrder,
                        };
                        _stateProvinceService.InsertStateProvince(state);
                    }

                    count++;
                }
            }

            //activity log
            _customerActivityService.InsertActivity("ImportStates", _localizationService.GetResource("ActivityLog.ImportStates"), count);

            return count;
        }

        /// <summary>
        /// Import categories from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual void ImportCategoriesFromXlsx(Stream stream)
        {
            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                //the columns
                var properties = GetPropertiesByExcelCells<Category>(worksheet);

                var manager = new PropertyManager<Category>(properties);

                var iRow = 2;
                var setSeName = properties.Any(p => p.PropertyName == "SeName");

                //performance optimization, load all categories in one SQL request
                var allCategories = _categoryService.GetAllCategories()
                    .GroupBy(c => c.GetFormattedBreadCrumb(_categoryService))
                    .ToDictionary(c => c.Key, c => c.First());

                var saveNextTime = new List<int>();

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                        .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                        .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    //get category by data in xlsx file if it possible, or create new category
                    var category = GetCategoryFromXlsx(manager, worksheet, iRow, allCategories, out bool isNew, out string curentCategoryBreadCrumb);

                    //update category by data in xlsx file
                    var seName = UpdateCategoryByXlsx(category, manager, allCategories, isNew, out bool isParentCategoryExists);

                    if (isParentCategoryExists)
                    {
                        //if parent category exists in database then save category into database
                        SaveCategory(isNew, category, allCategories, curentCategoryBreadCrumb, setSeName, seName);
                    }
                    else
                    {
                        //if parent category doesn't exists in database then try save category into database next time
                        saveNextTime.Add(iRow);
                    }

                    iRow++;
                }

                var needSave = saveNextTime.Any();

                while (needSave)
                {
                    var remove = new List<int>();

                    //try to save unsaved categories
                    foreach (var rowId in saveNextTime)
                    {
                        //get category by data in xlsx file if it possible, or create new category
                        var category = GetCategoryFromXlsx(manager, worksheet, rowId, allCategories, out bool isNew, out string curentCategoryBreadCrumb);
                        //update category by data in xlsx file
                        var seName = UpdateCategoryByXlsx(category, manager, allCategories, isNew, out bool isParentCategoryExists);

                        if (!isParentCategoryExists)
                            continue;

                        //if parent category exists in database then save category into database
                        SaveCategory(isNew, category, allCategories, curentCategoryBreadCrumb, setSeName, seName);
                        remove.Add(rowId);
                    }
                    
                    saveNextTime.RemoveAll(item => remove.Contains(item));

                    needSave = remove.Any() && saveNextTime.Any();
                }

                //activity log
                _customerActivityService.InsertActivity("ImportCategories", _localizationService.GetResource("ActivityLog.ImportCategories"), iRow - 2 - saveNextTime.Count);

                if (!saveNextTime.Any())
                    return;

                var caregoriesName = new List<string>();

                foreach (var rowId in saveNextTime)
                {
                    manager.ReadFromXlsx(worksheet, rowId);
                    caregoriesName.Add(manager.GetProperty("Name").StringValue);
                }

                throw new ArgumentException(string.Format(_localizationService.GetResource("Admin.Catalog.Categories.Import.CategoriesArentImported"), string.Join(", ", caregoriesName)));
            }
        }

        #endregion
        
    }
}
