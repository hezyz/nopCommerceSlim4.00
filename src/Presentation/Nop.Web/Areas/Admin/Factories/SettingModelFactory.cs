using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Services.Themes;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory implementation
    /// </summary>
    public partial class SettingModelFactory : ISettingModelFactory
    {
        #region Fields

        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerAttributeModelFactory _customerAttributeModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IFulltextService _fulltextService;
        private readonly IGdprService _gdprService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IPictureService _pictureService;
        private readonly IReviewTypeModelFactory _reviewTypeModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IThemeProvider _themeProvider;
        private readonly IVendorAttributeModelFactory _vendorAttributeModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SettingModelFactory(
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerAttributeModelFactory customerAttributeModelFactory,
            IDateTimeHelper dateTimeHelper,
            IFulltextService fulltextService,
            IGdprService gdprService,
            ILocalizedModelFactory localizedModelFactory,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IMaintenanceService maintenanceService,
            IPictureService pictureService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IThemeProvider themeProvider,
            IVendorAttributeModelFactory vendorAttributeModelFactory,
            IReviewTypeModelFactory reviewTypeModelFactory,
            IWorkContext workContext)
        {
            this._addressAttributeModelFactory = addressAttributeModelFactory;
            this._addressService = addressService;
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._customerAttributeModelFactory = customerAttributeModelFactory;
            this._dateTimeHelper = dateTimeHelper;
            this._fulltextService = fulltextService;
            this._gdprService = gdprService;
            this._localizedModelFactory = localizedModelFactory;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._maintenanceService = maintenanceService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._themeProvider = themeProvider;
            this._vendorAttributeModelFactory = vendorAttributeModelFactory;
            this._reviewTypeModelFactory = reviewTypeModelFactory;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //set some of address fields as enabled and required
            model.CountryEnabled = true;
            model.StateProvinceEnabled = true;
            model.CountyEnabled = true;
            model.CityEnabled = true;
            model.StreetAddressEnabled = true;
            model.ZipPostalCodeEnabled = true;
            model.ZipPostalCodeRequired = true;

            //prepare available countries
            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            //prepare available states
            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);
        }

        /// <summary>
        /// Prepare store theme models
        /// </summary>
        /// <param name="models">List of store theme models</param>
        protected virtual void PrepareStoreThemeModels(IList<StoreInformationSettingsModel.ThemeModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeId);

            //get available themes
            var availableThemes = _themeProvider.GetThemes();
            foreach (var theme in availableThemes)
            {
                models.Add(new StoreInformationSettingsModel.ThemeModel
                {
                    FriendlyName = theme.FriendlyName,
                    SystemName = theme.SystemName,
                    PreviewImageUrl = theme.PreviewImageUrl,
                    PreviewText = theme.PreviewText,
                    SupportRtl = theme.SupportRtl,
                    Selected = theme.SystemName.Equals(storeInformationSettings.DefaultStoreTheme, StringComparison.InvariantCultureIgnoreCase)
                });
            }
        }

        /// <summary>
        /// Prepare sort option search model
        /// </summary>
        /// <param name="searchModel">Sort option search model</param>
        /// <returns>Sort option search model</returns>
        protected virtual SortOptionSearchModel PrepareSortOptionSearchModel(SortOptionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare GDPR consent search model
        /// </summary>
        /// <param name="searchModel">GDPR consent search model</param>
        /// <returns>GDPR consent search model</returns>
        protected virtual GdprConsentSearchModel PrepareGdprConsentSearchModel(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare address settings model
        /// </summary>
        /// <returns>Address settings model</returns>
        protected virtual AddressSettingsModel PrepareAddressSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var addressSettings = _settingService.LoadSetting<AddressSettings>(storeId);

            //fill in model values from the entity
            var model = addressSettings.ToSettingsModel<AddressSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare customer settings model
        /// </summary>
        /// <returns>Customer settings model</returns>
        protected virtual CustomerSettingsModel PrepareCustomerSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeId);

            //fill in model values from the entity
            var model = customerSettings.ToSettingsModel<CustomerSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare date time settings model
        /// </summary>
        /// <returns>Date time settings model</returns>
        protected virtual DateTimeSettingsModel PrepareDateTimeSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeId);

            //fill in model values from the entity
            var model = new DateTimeSettingsModel
            {
                AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone
            };

            //fill in additional values (not existing in the entity)
            model.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;

            //prepare available time zones
            _baseAdminModelFactory.PrepareTimeZones(model.AvailableTimeZones, false);

            return model;
        }

        /// <summary>
        /// Prepare external authentication settings model
        /// </summary>
        /// <returns>External authentication settings model</returns>
        protected virtual ExternalAuthenticationSettingsModel PrepareExternalAuthenticationSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeId);

            //fill in model values from the entity
            var model = new ExternalAuthenticationSettingsModel
            {
                AllowCustomersToRemoveAssociations = externalAuthenticationSettings.AllowCustomersToRemoveAssociations
            };

            return model;
        }

        /// <summary>
        /// Prepare store information settings model
        /// </summary>
        /// <returns>Store information settings model</returns>
        protected virtual StoreInformationSettingsModel PrepareStoreInformationSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeId);
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new StoreInformationSettingsModel
            {
                StoreClosed = storeInformationSettings.StoreClosed,
                DefaultStoreTheme = storeInformationSettings.DefaultStoreTheme,
                AllowCustomerToSelectTheme = storeInformationSettings.AllowCustomerToSelectTheme,
                LogoPictureId = storeInformationSettings.LogoPictureId,
                DisplayEuCookieLawWarning = storeInformationSettings.DisplayEuCookieLawWarning,
                FacebookLink = storeInformationSettings.FacebookLink,
                TwitterLink = storeInformationSettings.TwitterLink,
                YoutubeLink = storeInformationSettings.YoutubeLink,
                GooglePlusLink = storeInformationSettings.GooglePlusLink,
                SubjectFieldOnContactUsForm = commonSettings.SubjectFieldOnContactUsForm,
                UseSystemEmailForContactUsForm = commonSettings.UseSystemEmailForContactUsForm,
                PopupForTermsOfServiceLinks = commonSettings.PopupForTermsOfServiceLinks,
                SitemapEnabled = commonSettings.SitemapEnabled,
                SitemapPageSize = commonSettings.SitemapPageSize,
                SitemapIncludeCategories = commonSettings.SitemapIncludeCategories,
                SitemapIncludeProducts = commonSettings.SitemapIncludeProducts,
                SitemapIncludeProductTags = commonSettings.SitemapIncludeProductTags
            };

            //prepare available themes
            PrepareStoreThemeModels(model.AvailableStoreThemes);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.StoreClosed_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.StoreClosed, storeId);
            model.DefaultStoreTheme_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.DefaultStoreTheme, storeId);
            model.AllowCustomerToSelectTheme_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeId);
            model.LogoPictureId_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.LogoPictureId, storeId);
            model.DisplayEuCookieLawWarning_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeId);
            model.FacebookLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.FacebookLink, storeId);
            model.TwitterLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.TwitterLink, storeId);
            model.YoutubeLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.YoutubeLink, storeId);
            model.GooglePlusLink_OverrideForStore = _settingService.SettingExists(storeInformationSettings, x => x.GooglePlusLink, storeId);
            model.SubjectFieldOnContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SubjectFieldOnContactUsForm, storeId);
            model.UseSystemEmailForContactUsForm_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.UseSystemEmailForContactUsForm, storeId);
            model.PopupForTermsOfServiceLinks_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.PopupForTermsOfServiceLinks, storeId);
            model.SitemapEnabled_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SitemapEnabled, storeId);
            model.SitemapPageSize_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SitemapPageSize, storeId);
            model.SitemapIncludeCategories_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SitemapIncludeCategories, storeId);
            model.SitemapIncludeProducts_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SitemapIncludeProducts, storeId);
            model.SitemapIncludeProductTags_OverrideForStore = _settingService.SettingExists(commonSettings, x => x.SitemapIncludeProductTags, storeId);

            return model;
        }

        /// <summary>
        /// Prepare SEO settings model
        /// </summary>
        /// <returns>SEO settings model</returns>
        protected virtual SeoSettingsModel PrepareSeoSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var seoSettings = _settingService.LoadSetting<SeoSettings>(storeId);

            //fill in model values from the entity
            var model = new SeoSettingsModel
            {
                PageTitleSeparator = seoSettings.PageTitleSeparator,
                PageTitleSeoAdjustment = (int)seoSettings.PageTitleSeoAdjustment,
                PageTitleSeoAdjustmentValues = seoSettings.PageTitleSeoAdjustment.ToSelectList(),
                DefaultTitle = seoSettings.DefaultTitle,
                DefaultMetaKeywords = seoSettings.DefaultMetaKeywords,
                DefaultMetaDescription = seoSettings.DefaultMetaDescription,
                GenerateProductMetaDescription = seoSettings.GenerateProductMetaDescription,
                ConvertNonWesternChars = seoSettings.ConvertNonWesternChars,
                CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled,
                WwwRequirement = (int)seoSettings.WwwRequirement,
                WwwRequirementValues = seoSettings.WwwRequirement.ToSelectList(),
                EnableJsBundling = seoSettings.EnableJsBundling,
                EnableCssBundling = seoSettings.EnableCssBundling,
                TwitterMetaTags = seoSettings.TwitterMetaTags,
                OpenGraphMetaTags = seoSettings.OpenGraphMetaTags,
                CustomHeadTags = seoSettings.CustomHeadTags
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.PageTitleSeparator_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.PageTitleSeparator, storeId);
            model.PageTitleSeoAdjustment_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.PageTitleSeoAdjustment, storeId);
            model.DefaultTitle_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultTitle, storeId);
            model.DefaultMetaKeywords_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultMetaKeywords, storeId);
            model.DefaultMetaDescription_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.DefaultMetaDescription, storeId);
            model.GenerateProductMetaDescription_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.GenerateProductMetaDescription, storeId);
            model.ConvertNonWesternChars_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.ConvertNonWesternChars, storeId);
            model.CanonicalUrlsEnabled_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.CanonicalUrlsEnabled, storeId);
            model.WwwRequirement_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.WwwRequirement, storeId);
            model.EnableJsBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableJsBundling, storeId);
            model.EnableCssBundling_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.EnableCssBundling, storeId);
            model.TwitterMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.TwitterMetaTags, storeId);
            model.OpenGraphMetaTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.OpenGraphMetaTags, storeId);
            model.CustomHeadTags_OverrideForStore = _settingService.SettingExists(seoSettings, x => x.CustomHeadTags, storeId);

            return model;
        }

        /// <summary>
        /// Prepare security settings model
        /// </summary>
        /// <returns>Security settings model</returns>
        protected virtual SecuritySettingsModel PrepareSecuritySettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeId);

            //fill in model values from the entity
            var model = new SecuritySettingsModel
            {
                EncryptionKey = securitySettings.EncryptionKey,
                ForceSslForAllPages = securitySettings.ForceSslForAllPages,
                EnableXsrfProtectionForAdminArea = securitySettings.EnableXsrfProtectionForAdminArea,
                EnableXsrfProtectionForPublicStore = securitySettings.EnableXsrfProtectionForPublicStore,
                HoneypotEnabled = securitySettings.HoneypotEnabled
            };

            //fill in additional values (not existing in the entity)
            if (securitySettings.AdminAreaAllowedIpAddresses != null)
                model.AdminAreaAllowedIpAddresses = string.Join(",", securitySettings.AdminAreaAllowedIpAddresses);

            return model;
        }

        /// <summary>
        /// Prepare captcha settings model
        /// </summary>
        /// <returns>Captcha settings model</returns>
        protected virtual CaptchaSettingsModel PrepareCaptchaSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeId);

            //fill in model values from the entity
            var model = captchaSettings.ToSettingsModel<CaptchaSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare localization settings model
        /// </summary>
        /// <returns>Localization settings model</returns>
        protected virtual LocalizationSettingsModel PrepareLocalizationSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeId);

            //fill in model values from the entity
            var model = new LocalizationSettingsModel
            {
                UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection,
                SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled,
                AutomaticallyDetectLanguage = localizationSettings.AutomaticallyDetectLanguage,
                LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup,
                LoadAllLocalizedPropertiesOnStartup = localizationSettings.LoadAllLocalizedPropertiesOnStartup,
                LoadAllUrlRecordsOnStartup = localizationSettings.LoadAllUrlRecordsOnStartup
            };

            return model;
        }

        /// <summary>
        /// Prepare full-text settings model
        /// </summary>
        /// <returns>Full-text settings model</returns>
        protected virtual FullTextSettingsModel PrepareFullTextSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var commonSettings = _settingService.LoadSetting<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new FullTextSettingsModel
            {
                Enabled = commonSettings.UseFullTextSearch,
                SearchMode = (int)commonSettings.FullTextMode
            };

            //fill in additional values (not existing in the entity)
            model.Supported = _fulltextService.IsFullTextSupported();
            model.SearchModeValues = commonSettings.FullTextMode.ToSelectList();

            return model;
        }

        /// <summary>
        /// Prepare admin area settings model
        /// </summary>
        /// <returns>Admin area settings model</returns>
        protected virtual AdminAreaSettingsModel PrepareAdminAreaSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var adminAreaSettings = _settingService.LoadSetting<AdminAreaSettings>(storeId);

            //fill in model values from the entity
            var model = new AdminAreaSettingsModel
            {
                UseRichEditorInMessageTemplates = adminAreaSettings.UseRichEditorInMessageTemplates
            };

            //fill in overridden values
            if (storeId > 0)
            {
                model.UseRichEditorInMessageTemplates_OverrideForStore = _settingService.SettingExists(adminAreaSettings, x => x.UseRichEditorInMessageTemplates, storeId);
            }

            return model;
        }

        /// <summary>
        /// Prepare display default menu item settings model
        /// </summary>
        /// <returns>Display default menu item settings model</returns>
        protected virtual DisplayDefaultMenuItemSettingsModel PrepareDisplayDefaultMenuItemSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var displayDefaultMenuItemSettings = _settingService.LoadSetting<DisplayDefaultMenuItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultMenuItemSettingsModel
            {
                DisplayHomePageMenuItem = displayDefaultMenuItemSettings.DisplayHomePageMenuItem,
                DisplayNewProductsMenuItem = displayDefaultMenuItemSettings.DisplayNewProductsMenuItem,
                DisplayProductSearchMenuItem = displayDefaultMenuItemSettings.DisplayProductSearchMenuItem,
                DisplayCustomerInfoMenuItem = displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem,
                DisplayBlogMenuItem = displayDefaultMenuItemSettings.DisplayBlogMenuItem,
                DisplayForumsMenuItem = displayDefaultMenuItemSettings.DisplayForumsMenuItem,
                DisplayContactUsMenuItem = displayDefaultMenuItemSettings.DisplayContactUsMenuItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplayHomePageMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayHomePageMenuItem, storeId);
            model.DisplayNewProductsMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayNewProductsMenuItem, storeId);
            model.DisplayProductSearchMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayProductSearchMenuItem, storeId);
            model.DisplayCustomerInfoMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayCustomerInfoMenuItem, storeId);
            model.DisplayBlogMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayBlogMenuItem, storeId);
            model.DisplayForumsMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayForumsMenuItem, storeId);
            model.DisplayContactUsMenuItem_OverrideForStore = _settingService.SettingExists(displayDefaultMenuItemSettings, x => x.DisplayContactUsMenuItem, storeId);

            return model;
        }

        /// <summary>
        /// Prepare display default footer item settings model
        /// </summary>
        /// <returns>Display default footer item settings model</returns>
        protected virtual DisplayDefaultFooterItemSettingsModel PrepareDisplayDefaultFooterItemSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var displayDefaultFooterItemSettings = _settingService.LoadSetting<DisplayDefaultFooterItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultFooterItemSettingsModel
            {
                DisplaySitemapFooterItem = displayDefaultFooterItemSettings.DisplaySitemapFooterItem,
                DisplayContactUsFooterItem = displayDefaultFooterItemSettings.DisplayContactUsFooterItem,
                DisplayProductSearchFooterItem = displayDefaultFooterItemSettings.DisplayProductSearchFooterItem,
                DisplayNewsFooterItem = displayDefaultFooterItemSettings.DisplayNewsFooterItem,
                DisplayBlogFooterItem = displayDefaultFooterItemSettings.DisplayBlogFooterItem,
                DisplayForumsFooterItem = displayDefaultFooterItemSettings.DisplayForumsFooterItem,
                DisplayRecentlyViewedProductsFooterItem = displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem,
                DisplayNewProductsFooterItem = displayDefaultFooterItemSettings.DisplayNewProductsFooterItem,
                DisplayCustomerInfoFooterItem = displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem,
                DisplayCustomerAddressesFooterItem = displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem,
                DisplayApplyVendorAccountFooterItem = displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplaySitemapFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplaySitemapFooterItem, storeId);
            model.DisplayContactUsFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayContactUsFooterItem, storeId);
            model.DisplayProductSearchFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayProductSearchFooterItem, storeId);
            model.DisplayNewsFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayNewsFooterItem, storeId);
            model.DisplayBlogFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayBlogFooterItem, storeId);
            model.DisplayForumsFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayForumsFooterItem, storeId);
            model.DisplayRecentlyViewedProductsFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayRecentlyViewedProductsFooterItem, storeId);
            model.DisplayNewProductsFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayNewProductsFooterItem, storeId);
            model.DisplayCustomerInfoFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayCustomerInfoFooterItem, storeId);
            model.DisplayCustomerAddressesFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayCustomerAddressesFooterItem, storeId);
            model.DisplayApplyVendorAccountFooterItem_OverrideForStore = _settingService.SettingExists(displayDefaultFooterItemSettings, x => x.DisplayApplyVendorAccountFooterItem, storeId);

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare blog settings model
        /// </summary>
        /// <returns>Blog settings model</returns>
        public virtual BlogSettingsModel PrepareBlogSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var blogSettings = _settingService.LoadSetting<BlogSettings>(storeId);

            //fill in model values from the entity
            var model = blogSettings.ToSettingsModel<BlogSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.Enabled_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.Enabled, storeId);
            model.PostsPageSize_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.PostsPageSize, storeId);
            model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeId);
            model.NotifyAboutNewBlogComments_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.NotifyAboutNewBlogComments, storeId);
            model.NumberOfTags_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.NumberOfTags, storeId);
            model.ShowHeaderRssUrl_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.ShowHeaderRssUrl, storeId);
            model.BlogCommentsMustBeApproved_OverrideForStore = _settingService.SettingExists(blogSettings, x => x.BlogCommentsMustBeApproved, storeId);

            return model;
        }

        /// <summary>
        /// Prepare vendor settings model
        /// </summary>
        /// <returns>Vendor settings model</returns>
        public virtual VendorSettingsModel PrepareVendorSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var vendorSettings = _settingService.LoadSetting<VendorSettings>(storeId);

            //fill in model values from the entity
            var model = vendorSettings.ToSettingsModel<VendorSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            //fill in overridden values
            if (storeId > 0)
            {
                model.VendorsBlockItemsToDisplay_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.VendorsBlockItemsToDisplay, storeId);
                model.ShowVendorOnProductDetailsPage_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.ShowVendorOnProductDetailsPage, storeId);
                model.ShowVendorOnOrderDetailsPage_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.ShowVendorOnOrderDetailsPage, storeId);
                model.AllowCustomersToContactVendors_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowCustomersToContactVendors, storeId);
                model.AllowCustomersToApplyForVendorAccount_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, storeId);
                model.TermsOfServiceEnabled_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.TermsOfServiceEnabled, storeId);
                model.AllowSearchByVendor_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowSearchByVendor, storeId);
                model.AllowVendorsToEditInfo_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowVendorsToEditInfo, storeId);
                model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, storeId);
                model.MaximumProductNumber_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.MaximumProductNumber, storeId);
                model.AllowVendorsToImportProducts_OverrideForStore = _settingService.SettingExists(vendorSettings, x => x.AllowVendorsToImportProducts, storeId);
            }

            //prepare nested search model
            _vendorAttributeModelFactory.PrepareVendorAttributeSearchModel(model.VendorAttributeSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare forum settings model
        /// </summary>
        /// <returns>Forum settings model</returns>
        public virtual ForumSettingsModel PrepareForumSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var forumSettings = _settingService.LoadSetting<ForumSettings>(storeId);

            //fill in model values from the entity
            var model = forumSettings.ToSettingsModel<ForumSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList();

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.ForumsEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumsEnabled, storeId);
            model.RelativeDateTimeFormattingEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.RelativeDateTimeFormattingEnabled, storeId);
            model.ShowCustomersPostCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ShowCustomersPostCount, storeId);
            model.AllowGuestsToCreatePosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowGuestsToCreatePosts, storeId);
            model.AllowGuestsToCreateTopics_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowGuestsToCreateTopics, storeId);
            model.AllowCustomersToEditPosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToEditPosts, storeId);
            model.AllowCustomersToDeletePosts_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToDeletePosts, storeId);
            model.AllowPostVoting_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowPostVoting, storeId);
            model.MaxVotesPerDay_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.MaxVotesPerDay, storeId);
            model.AllowCustomersToManageSubscriptions_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowCustomersToManageSubscriptions, storeId);
            model.TopicsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.TopicsPageSize, storeId);
            model.PostsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.PostsPageSize, storeId);
            model.ForumEditor_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumEditor, storeId);
            model.SignaturesEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.SignaturesEnabled, storeId);
            model.AllowPrivateMessages_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.AllowPrivateMessages, storeId);
            model.ShowAlertForPM_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ShowAlertForPM, storeId);
            model.NotifyAboutPrivateMessages_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.NotifyAboutPrivateMessages, storeId);
            model.ActiveDiscussionsFeedEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsFeedEnabled, storeId);
            model.ActiveDiscussionsFeedCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsFeedCount, storeId);
            model.ForumFeedsEnabled_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumFeedsEnabled, storeId);
            model.ForumFeedCount_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ForumFeedCount, storeId);
            model.SearchResultsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.SearchResultsPageSize, storeId);
            model.ActiveDiscussionsPageSize_OverrideForStore = _settingService.SettingExists(forumSettings, x => x.ActiveDiscussionsPageSize, storeId);

            return model;
        }

        /// <summary>
        /// Prepare news settings model
        /// </summary>
        /// <returns>News settings model</returns>
        public virtual NewsSettingsModel PrepareNewsSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var newsSettings = _settingService.LoadSetting<NewsSettings>(storeId);

            //fill in model values from the entity
            var model = newsSettings.ToSettingsModel<NewsSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.Enabled_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.Enabled, storeId);
            model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeId);
            model.NotifyAboutNewNewsComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NotifyAboutNewNewsComments, storeId);
            model.ShowNewsOnMainPage_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowNewsOnMainPage, storeId);
            model.MainPageNewsCount_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.MainPageNewsCount, storeId);
            model.NewsArchivePageSize_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NewsArchivePageSize, storeId);
            model.ShowHeaderRssUrl_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowHeaderRssUrl, storeId);
            model.NewsCommentsMustBeApproved_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NewsCommentsMustBeApproved, storeId);

            return model;
        }

        /// <summary>
        /// Prepare catalog settings model
        /// </summary>
        /// <returns>Catalog settings model</returns>
        public virtual CatalogSettingsModel PrepareCatalogSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeId);

            //fill in model values from the entity
            var model = catalogSettings.ToSettingsModel<CatalogSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            model.AvailableViewModes.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.ViewMode.Grid"),
                Value = "grid"
            });
            model.AvailableViewModes.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.ViewMode.List"),
                Value = "list"
            });

            //fill in overridden values
            if (storeId > 0)
            {
                model.AllowViewUnpublishedProductPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowViewUnpublishedProductPage, storeId);
                model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, storeId);
                model.AllowProductSorting_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductSorting, storeId);
                model.AllowProductViewModeChanging_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowProductViewModeChanging, storeId);
                model.DefaultViewMode_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.DefaultViewMode, storeId);
                model.ShowProductsFromSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductsFromSubcategories, storeId);
                model.ShowCategoryProductNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumber, storeId);
                model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, storeId);
                model.CategoryBreadcrumbEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.CategoryBreadcrumbEnabled, storeId);
                model.ShowShareButton_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowShareButton, storeId);
                model.PageShareCode_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.PageShareCode, storeId);
                model.ProductReviewsMustBeApproved_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewsMustBeApproved, storeId);
                model.AllowAnonymousUsersToReviewProduct_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, storeId);
                model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, storeId);
                model.NotifyCustomerAboutProductReviewReply_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NotifyCustomerAboutProductReviewReply, storeId);
                model.EmailAFriendEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.EmailAFriendEnabled, storeId);
                model.AllowAnonymousUsersToEmailAFriend_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, storeId);
                model.RecentlyViewedProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsNumber, storeId);
                model.RecentlyViewedProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.RecentlyViewedProductsEnabled, storeId);
                model.NewProductsNumber_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsNumber, storeId);
                model.NewProductsEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NewProductsEnabled, storeId);
                model.SearchPageProductsPerPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageProductsPerPage, storeId);
                model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, storeId);
                model.SearchPagePageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.SearchPagePageSizeOptions, storeId);
                model.ProductSearchAutoCompleteEnabled_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, storeId);
                model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, storeId);
                model.ShowProductImagesInSearchAutoComplete_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, storeId);
                model.ShowLinkToAllResultInSearchAutoComplete_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowLinkToAllResultInSearchAutoComplete, storeId);
                model.ProductSearchTermMinimumLength_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductSearchTermMinimumLength, storeId);
                model.NumberOfProductTags_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.NumberOfProductTags, storeId);
                model.ProductsByTagPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSize, storeId);
                model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, storeId);
                model.ProductsByTagPageSizeOptions_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductsByTagPageSizeOptions, storeId);
                model.ShowProductReviewsPerStore_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductReviewsPerStore, storeId);
                model.ShowProductReviewsOnAccountPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ShowProductReviewsTabOnAccountPage, storeId);
                model.ProductReviewsPageSizeOnAccountPage_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewsPageSizeOnAccountPage, storeId);
                model.ProductReviewsSortByCreatedDateAscending_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ProductReviewsSortByCreatedDateAscending, storeId);
                model.ExportImportProductCategoryBreadcrumb_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ExportImportProductCategoryBreadcrumb, storeId);
                model.ExportImportCategoriesUsingCategoryName_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ExportImportCategoriesUsingCategoryName, storeId);
                model.ExportImportAllowDownloadImages_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ExportImportAllowDownloadImages, storeId);
                model.ExportImportSplitProductsFile_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ExportImportSplitProductsFile, storeId);
                model.ExportImportRelatedEntitiesByName_OverrideForStore = _settingService.SettingExists(catalogSettings, x => x.ExportImportRelatedEntitiesByName, storeId);
            }

            //prepare nested search model
            PrepareSortOptionSearchModel(model.SortOptionSearchModel);
            _reviewTypeModelFactory.PrepareReviewTypeSearchModel(model.ReviewTypeSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare paged sort option list model
        /// </summary>
        /// <param name="searchModel">Sort option search model</param>
        /// <returns>Sort option list model</returns>
        public virtual SortOptionListModel PrepareSortOptionListModel(SortOptionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeId);

            //get sort options
            var sortOptions = Enum.GetValues(typeof(ProductSortingEnum)).OfType<ProductSortingEnum>().ToList();

            //prepare list model
            var model = new SortOptionListModel
            {
                Data = sortOptions.PaginationByRequestModel(searchModel).Select(option =>
                {
                    //fill in model values from the entity
                    var sortOptionModel = new SortOptionModel
                    {
                        Id = (int)option
                    };

                    //fill in additional values (not existing in the entity)
                    sortOptionModel.Name = _localizationService.GetLocalizedEnum(option);
                    sortOptionModel.IsActive = !catalogSettings.ProductSortingEnumDisabled.Contains((int)option);
                    sortOptionModel.DisplayOrder = catalogSettings
                        .ProductSortingEnumDisplayOrder.TryGetValue((int)option, out var value) ? value : (int)option;

                    return sortOptionModel;
                }).OrderBy(option => option.DisplayOrder).ToList(),
                Total = sortOptions.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare media settings model
        /// </summary>
        /// <returns>Media settings model</returns>
        public virtual MediaSettingsModel PrepareMediaSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeId);

            //fill in model values from the entity
            var model = mediaSettings.ToSettingsModel<MediaSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            model.PicturesStoredIntoDatabase = _pictureService.StoreInDb;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.AvatarPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.AvatarPictureSize, storeId);
            model.ProductThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductThumbPictureSize, storeId);
            model.ProductDetailsPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductDetailsPictureSize, storeId);
            model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage, storeId);
            model.AssociatedProductPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.AssociatedProductPictureSize, storeId);
            model.CategoryThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.CategoryThumbPictureSize, storeId);
            model.VendorThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.VendorThumbPictureSize, storeId);
            model.MaximumImageSize_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.MaximumImageSize, storeId);
            model.MultipleThumbDirectories_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.MultipleThumbDirectories, storeId);
            model.DefaultImageQuality_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.DefaultImageQuality, storeId);
            model.ImportProductImagesUsingHash_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.ImportProductImagesUsingHash, storeId);
            model.DefaultPictureZoomEnabled_OverrideForStore = _settingService.SettingExists(mediaSettings, x => x.DefaultPictureZoomEnabled, storeId);

            return model;
        }

        /// <summary>
        /// Prepare customer user settings model
        /// </summary>
        /// <returns>Customer user settings model</returns>
        public virtual CustomerUserSettingsModel PrepareCustomerUserSettingsModel()
        {
            var model = new CustomerUserSettingsModel
            {
                ActiveStoreScopeConfiguration = _storeContext.ActiveStoreScopeConfiguration
            };

            //prepare customer settings model
            model.CustomerSettings = PrepareCustomerSettingsModel();

            //prepare address settings model
            model.AddressSettings = PrepareAddressSettingsModel();

            //prepare date time settings model
            model.DateTimeSettings = PrepareDateTimeSettingsModel();

            //prepare external authentication settings model
            model.ExternalAuthenticationSettings = PrepareExternalAuthenticationSettingsModel();

            //prepare nested search models
            _customerAttributeModelFactory.PrepareCustomerAttributeSearchModel(model.CustomerAttributeSearchModel);
            _addressAttributeModelFactory.PrepareAddressAttributeSearchModel(model.AddressAttributeSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare GDPR settings model
        /// </summary>
        /// <returns>GDPR settings model</returns>
        public virtual GdprSettingsModel PrepareGdprSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var gdprSettings = _settingService.LoadSetting<GdprSettings>(storeId);

            //fill in model values from the entity
            var model = gdprSettings.ToSettingsModel<GdprSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.GdprEnabled_OverrideForStore = _settingService.SettingExists(gdprSettings, x => x.GdprEnabled, storeId);
            model.LogPrivacyPolicyConsent_OverrideForStore = _settingService.SettingExists(gdprSettings, x => x.LogPrivacyPolicyConsent, storeId);
            model.LogNewsletterConsent_OverrideForStore = _settingService.SettingExists(gdprSettings, x => x.LogNewsletterConsent, storeId);

            //prepare nested search model
            PrepareGdprConsentSearchModel(model.GdprConsentSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare paged GDPR consent list model
        /// </summary>
        /// <param name="searchModel">GDPR search model</param>
        /// <returns>GDPR consent list model</returns>
        public virtual GdprConsentListModel PrepareGdprConsentListModel(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sort options
            var consentList = _gdprService.GetAllConsents();

            //prepare list model
            var model = new GdprConsentListModel
            {
                Data = consentList.PaginationByRequestModel(searchModel).Select(consent =>
                {
                    var gdprConsentModel = consent.ToModel<GdprConsentModel>();
                    var gdprConsent = _gdprService.GetConsentById(gdprConsentModel.Id);
                    gdprConsentModel.Message = _localizationService.GetLocalized(gdprConsent, entity => entity.Message);
                    gdprConsentModel.RequiredMessage = _localizationService.GetLocalized(gdprConsent, entity => entity.RequiredMessage);

                    return gdprConsentModel;
                }),
                Total = consentList.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare GDPR consent model
        /// </summary>
        /// <param name="model">GDPR consent model</param>
        /// <param name="gdprConsent">GDPR consent</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>GDPR consent model</returns>
        public virtual GdprConsentModel PrepareGdprConsentModel(GdprConsentModel model, GdprConsent gdprConsent, bool excludeProperties = false)
        {
            Action<GdprConsentLocalizedModel, int> localizedModelConfiguration = null;

            //fill in model values from the entity
            if (gdprConsent != null)
            {
                model = model ?? gdprConsent.ToModel<GdprConsentModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Message = _localizationService.GetLocalized(gdprConsent, entity => entity.Message, languageId, false, false);
                    locale.RequiredMessage = _localizationService.GetLocalized(gdprConsent, entity => entity.RequiredMessage, languageId, false, false);
                };
            }

            //set default values for the new model
            if (gdprConsent == null)
                model.DisplayOrder = 1;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare general and common settings model
        /// </summary>
        /// <returns>General and common settings model</returns>
        public virtual GeneralCommonSettingsModel PrepareGeneralCommonSettingsModel()
        {
            var model = new GeneralCommonSettingsModel
            {
                ActiveStoreScopeConfiguration = _storeContext.ActiveStoreScopeConfiguration
            };

            //prepare store information settings model
            model.StoreInformationSettings = PrepareStoreInformationSettingsModel();

            //prepare SEO settings model
            model.SeoSettings = PrepareSeoSettingsModel();

            //prepare security settings model
            model.SecuritySettings = PrepareSecuritySettingsModel();

            //prepare captcha settings model
            model.CaptchaSettings = PrepareCaptchaSettingsModel();

            //prepare PDF settings model
            model.LocalizationSettings = PrepareLocalizationSettingsModel();

            //prepare full-text settings model
            model.FullTextSettings = PrepareFullTextSettingsModel();

            //prepare admin area settings model
            model.AdminAreaSettings = PrepareAdminAreaSettingsModel();

            //prepare display default menu item settings model
            model.DisplayDefaultMenuItemSettings = PrepareDisplayDefaultMenuItemSettingsModel();

            //prepare display default footer item settings model
            model.DisplayDefaultFooterItemSettings = PrepareDisplayDefaultFooterItemSettingsModel();

            return model;
        }

        /// <summary>
        /// Prepare product editor settings model
        /// </summary>
        /// <returns>Product editor settings model</returns>
        public virtual ProductEditorSettingsModel PrepareProductEditorSettingsModel()
        {
            //load settings for a chosen store scope
            var storeId = _storeContext.ActiveStoreScopeConfiguration;
            var productEditorSettings = _settingService.LoadSetting<ProductEditorSettings>(storeId);

            //fill in model values from the entity
            var model = productEditorSettings.ToSettingsModel<ProductEditorSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare setting search model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>Setting search model</returns>
        public virtual SettingSearchModel PrepareSettingSearchModel(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged setting list model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>Setting list model</returns>
        public virtual SettingListModel PrepareSettingListModel(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get settings
            var settings = _settingService.GetAllSettings().AsQueryable();

            //filter settings
            //TODO: move filter to setting service
            if (!string.IsNullOrEmpty(searchModel.SearchSettingName))
                settings = settings.Where(setting => setting.Name.ToLowerInvariant().Contains(searchModel.SearchSettingName.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(searchModel.SearchSettingValue))
                settings = settings.Where(setting => setting.Value.ToLowerInvariant().Contains(searchModel.SearchSettingValue.ToLowerInvariant()));

            //prepare list model
            var model = new SettingListModel
            {
                Data = settings.PaginationByRequestModel(searchModel).Select(setting =>
                {
                    //fill in model values from the entity
                    var settingModel = new SettingModel
                    {
                        Id = setting.Id,
                        Name = setting.Name,
                        Value = setting.Value,
                        StoreId = setting.StoreId
                    };

                    //fill in additional values (not existing in the entity)
                    settingModel.Store = setting.StoreId > 0
                        ? _storeService.GetStoreById(setting.StoreId)?.Name ?? "Deleted"
                        : _localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");

                    return settingModel;
                }),

                Total = settings.Count()
            };

            return model;
        }

        /// <summary>
        /// Prepare setting mode model
        /// </summary>
        /// <param name="modeName">Mode name</param>
        /// <returns>Setting mode model</returns>
        public virtual SettingModeModel PrepareSettingModeModel(string modeName)
        {
            var model = new SettingModeModel
            {
                ModeName = modeName,
                Enabled = _genericAttributeService.GetAttribute<bool>(_workContext.CurrentCustomer, modeName)
            };

            return model;
        }

        /// <summary>
        /// Prepare store scope configuration model
        /// </summary>
        /// <returns>Store scope configuration model</returns>
        public virtual StoreScopeConfigurationModel PrepareStoreScopeConfigurationModel()
        {
            var model = new StoreScopeConfigurationModel
            {
                Stores = _storeService.GetAllStores().Select(store => store.ToModel<StoreModel>()).ToList(),
                StoreId = _storeContext.ActiveStoreScopeConfiguration
            };

            return model;
        }

        #endregion
    }
}