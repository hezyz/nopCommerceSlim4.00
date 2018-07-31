using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly MediaSettings _mediaSettings;
        
        #endregion

        #region Ctor

        public CustomerModelFactory(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            GdprSettings gdprSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService,
            IStoreContext storeContext,
            IStoreService storeService,
            MediaSettings mediaSettings)
        {
            this._addressSettings = addressSettings;
            this._customerSettings = customerSettings;
            this._dateTimeSettings = dateTimeSettings;
            this._gdprSettings = gdprSettings;
            this._aclSupportedModelFactory = aclSupportedModelFactory;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._addressAttributeModelFactory = addressAttributeModelFactory;
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._customerActivityService = customerActivityService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._externalAuthenticationService = externalAuthenticationService;
            this._gdprService = gdprService;
            this._genericAttributeService = genericAttributeService;
            this._geoLookupService = geoLookupService;
            this._localizationService = localizationService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._pictureService = pictureService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare customer associated external authorization models
        /// </summary>
        /// <param name="models">List of customer associated external authorization models</param>
        /// <param name="customer">Customer</param>
        protected virtual void PrepareAssociatedExternalAuthModels(IList<CustomerAssociatedExternalAuthModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            foreach (var record in customer.ExternalAuthenticationRecords)
            {
                var method = _externalAuthenticationService.LoadExternalAuthenticationMethodBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                models.Add(new CustomerAssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = !string.IsNullOrEmpty(record.ExternalDisplayIdentifier)
                        ? record.ExternalDisplayIdentifier : record.ExternalIdentifier,
                    AuthMethodName = method.PluginDescriptor.FriendlyName
                });
            }
        }

        /// <summary>
        /// Prepare customer attribute models
        /// </summary>
        /// <param name="models">List of customer attribute models</param>
        /// <param name="customer">Customer</param>
        protected virtual void PrepareCustomerAttributeModels(IList<CustomerModel.CustomerAttributeModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //get available customer attributes
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                if (customer != null)
                {
                    var selectedCustomerAttributes = _genericAttributeService
                        .GetAttribute<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            {
                                if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedCustomerAttributes);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //do nothing
                                //values are already pre-set
                            }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    var enteredText = _customerAttributeParser.ParseValues(selectedCustomerAttributes, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        case AttributeControlType.FileUpload:
                        default:
                            //not supported attribute control types
                            break;
                    }
                }

                models.Add(attributeModel);
            }
        }

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
            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.CompanyRequired = _addressSettings.CompanyRequired;
            model.CountryEnabled = _addressSettings.CountryEnabled;
            model.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
            model.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.CityEnabled = _addressSettings.CityEnabled;
            model.CityRequired = _addressSettings.CityRequired;
            model.CountyEnabled = _addressSettings.CountyEnabled;
            model.CountyRequired = _addressSettings.CountyRequired;
            model.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.PhoneRequired = _addressSettings.PhoneRequired;
            model.FaxEnabled = _addressSettings.FaxEnabled;
            model.FaxRequired = _addressSettings.FaxRequired;

            //prepare available countries
            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            //prepare available states
            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);

            //prepare custom address attributes
            _addressAttributeModelFactory.PrepareCustomAddressAttributes(model.CustomAddressAttributes, address);
        }

        /// <summary>
        /// Prepare HTML string address
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        protected virtual void PrepareModelAddressHtml(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var addressHtmlSb = new StringBuilder("<div>");

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (_addressSettings.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = _addressAttributeFormatter.FormatAttributes(address?.CustomAttributes);
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                //already encoded
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");

            model.AddressHtml = addressHtmlSb.ToString();
        }

        /// <summary>
        /// Prepare customer address search model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address search model</returns>
        protected virtual CustomerAddressSearchModel PrepareCustomerAddressSearchModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare customer activity log search model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log search model</returns>
        protected virtual CustomerActivityLogSearchModel PrepareCustomerActivityLogSearchModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare customer search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer search model</returns>
        public virtual CustomerSearchModel PrepareCustomerSearchModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            searchModel.AvatarEnabled = _customerSettings.AllowCustomersToUploadAvatars;
            searchModel.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;
            searchModel.PhoneEnabled = _customerSettings.PhoneEnabled;
            searchModel.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;

            //search registered customers by default
            var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            //prepare available customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged customer list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer list model</returns>
        public virtual CustomerListModel PrepareCustomerListModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
            int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

            //get customers
            var customers = _customerService.GetAllCustomers(
                customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                username: searchModel.SearchUsername,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                dayOfBirth: dayOfBirth,
                monthOfBirth: monthOfBirth,
                company: searchModel.SearchCompany,
                phone: searchModel.SearchPhone,
                zipPostalCode: searchModel.SearchZipPostalCode,
                ipAddress: searchModel.SearchIpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerListModel
            {
                Data = customers.Select(customer =>
                {
                    //fill in model values from the entity
                    var customerModel = new CustomerModel
                    {
                        Id = customer.Id,
                        Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                        Username = customer.Username,
                        FullName = _customerService.GetCustomerFullName(customer),
                        Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                        Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                        ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                        Active = customer.Active
                    };

                    //convert dates to the user time
                    customerModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    customerModel.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerRoleNames = string.Join(", ", customer.CustomerRoles.Select(role => role.Name));
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var avatarPictureId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                        customerModel.AvatarUrl = _pictureService.GetPictureUrl(avatarPictureId, _mediaSettings.AvatarPictureSize,
                            _customerSettings.DefaultAvatarEnabled, defaultPictureType: PictureType.Avatar);
                    }

                    return customerModel;
                }),
                Total = customers.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare customer model
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer model</returns>
        public virtual CustomerModel PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties = false)
        {
            if (customer != null)
            {
                //fill in model values from the entity
                model = model ?? new CustomerModel();

                model.Id = customer.Id;
                model.AllowSendingOfWelcomeMessage = customer.IsRegistered() &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval;
                model.AllowReSendingOfActivationMessage = customer.IsRegistered() && !customer.Active &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                model.GdprEnabled = _gdprSettings.GdprEnabled;

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.Active = customer.Active;
                    model.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    model.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    model.Gender = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.GenderAttribute);
                    model.DateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                    model.Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    model.StreetAddress = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                    model.StreetAddress2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute);
                    model.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                    model.City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute);
                    model.County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute);
                    model.CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                    model.StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute);
                    model.Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    model.Fax = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute);
                    model.TimeZoneId = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.TimeZoneIdAttribute);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.LastVisitedPage = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute);
                    model.SelectedCustomerRoleIds = customer.CustomerCustomerRoleMappings.Select(mapping => mapping.CustomerRoleId).ToList();
                    model.RegisteredInStore = _storeService.GetAllStores()
                        .FirstOrDefault(store => store.Id == customer.RegisteredInStoreId)?.Name ?? string.Empty;

                    //prepare model newsletter subscriptions
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        model.SelectedNewsletterSubscriptionStoreIds = _storeService.GetAllStores()
                            .Where(store => _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id) != null)
                            .Select(store => store.Id).ToList();
                    }
                }
               
                //prepare external authentication records
                PrepareAssociatedExternalAuthModels(model.AssociatedExternalAuthRecords, customer);

                //prepare nested search models
                PrepareCustomerAddressSearchModel(model.CustomerAddressSearchModel, customer);
                PrepareCustomerActivityLogSearchModel(model.CustomerActivityLogSearchModel, customer);
            }
            else
            {
                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    //precheck Registered Role as a default role while creating a new customer through admin
                    var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
                    if (registeredRole != null)
                        model.SelectedCustomerRoleIds.Add(registeredRole.Id);
                }
            }

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //set default values for the new model
            if (customer == null)
            {
                model.Active = true;
            }

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: _localizationService.GetResource("Admin.Customers.Customers.Fields.Vendor.None"));

            //prepare model customer attributes
            PrepareCustomerAttributeModels(model.CustomerAttributes, customer);

            //prepare model stores for newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = _storeService.GetAllStores().Select(store => new SelectListItem
            {
                Value = store.Id.ToString(),
                Text = store.Name,
                Selected = model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id)
            }).ToList();

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model);

            //prepare available time zones
            _baseAdminModelFactory.PrepareTimeZones(model.AvailableTimeZones, false);

            //prepare available countries and states
            if (_customerSettings.CountryEnabled)
            {
                _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);
                if (_customerSettings.StateProvinceEnabled)
                    _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId == 0 ? null : (int?)model.CountryId);
            }

            return model;
        }

        /// <summary>
        /// Prepare paged customer address list model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address list model</returns>
        public virtual CustomerAddressListModel PrepareCustomerAddressListModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer addresses
            var addresses = customer.Addresses
                .OrderByDescending(address => address.CreatedOnUtc).ThenByDescending(address => address.Id).ToList();

            //prepare list model
            var model = new CustomerAddressListModel
            {
                Data = addresses.PaginationByRequestModel(searchModel).Select(address =>
                {
                    //fill in model values from the entity        
                    var addressModel = address.ToModel<AddressModel>();

                    //fill in additional values (not existing in the entity)
                    PrepareModelAddressHtml(addressModel, address);

                    return addressModel;
                }),
                Total = addresses.Count
            };

            return model;
        }

        /// <summary>
        /// Prepare customer address model
        /// </summary>
        /// <param name="model">Customer address model</param>
        /// <param name="customer">Customer</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer address model</returns>
        public virtual CustomerAddressModel PrepareCustomerAddressModel(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (address != null)
            {
                //fill in model values from the entity
                model = model ?? new CustomerAddressModel();

                //whether to fill in some of properties
                if (!excludeProperties)
                    model.Address = address.ToModel(model.Address);
            }

            model.CustomerId = customer.Id;

            //prepare address model
            PrepareAddressModel(model.Address, address);

            return model;
        }

        /// <summary>
        /// Prepare paged customer activity log list model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log list model</returns>
        public virtual CustomerActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer activity log
            var activityLog = _customerActivityService.GetAllActivities(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerActivityLogListModel
            {
                Data = activityLog.Select(logItem =>
                {
                    //fill in model values from the entity
                    var customerActivityLogModel = new CustomerActivityLogModel
                    {
                        Id = logItem.Id,
                        ActivityLogTypeName = logItem.ActivityLogType.Name,
                        Comment = logItem.Comment,
                        IpAddress = logItem.IpAddress
                    };

                    //convert dates to the user time
                    customerActivityLogModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc);

                    return customerActivityLogModel;
                }),
                Total = activityLog.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare online customer search model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer search model</returns>
        public virtual OnlineCustomerSearchModel PrepareOnlineCustomerSearchModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged online customer list model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer list model</returns>
        public virtual OnlineCustomerListModel PrepareOnlineCustomerListModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes);

            //get online customers
            var customers = _customerService.GetOnlineCustomers(customerRoleIds: null,
                 lastActivityFromUtc: lastActivityFrom,
                 pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new OnlineCustomerListModel
            {
                Data = customers.Select(customer =>
                {
                    //fill in model values from the entity
                    var customerModel = new OnlineCustomerModel
                    {
                        Id = customer.Id
                    };

                    //convert dates to the user time
                    customerModel.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerInfo = customer.IsRegistered()
                        ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    customerModel.LastIpAddress = _customerSettings.StoreIpAddresses
                        ? customer.LastIpAddress : _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.IPAddress.Disabled");
                    customerModel.Location = _geoLookupService.LookupCountryName(customer.LastIpAddress);
                    customerModel.LastVisitedPage = _customerSettings.StoreLastVisitedPage
                        ? _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute)
                        : _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled");

                    return customerModel;
                }),
                Total = customers.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare GDPR request (log) search model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request search model</returns>
        public virtual GdprLogSearchModel PrepareGdprLogSearchModel(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare request types
            _baseAdminModelFactory.PrepareGdprRequestTypes(searchModel.AvailableRequestTypes);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged GDPR request list model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request list model</returns>
        public virtual GdprLogListModel PrepareGdprLogListModel(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerId = 0;
            var customerInfo = "";
            if (!String.IsNullOrEmpty(searchModel.SearchEmail))
            {
                var customer = _customerService.GetCustomerByEmail(searchModel.SearchEmail);
                if (customer != null)
                    customerId = customer.Id;
                else
                {
                    customerInfo = searchModel.SearchEmail;
                }
            }
            //get requests
            var gdprLog = _gdprService.GetAllLog(
                customerId: customerId,
                customerInfo: customerInfo,
                requestType: searchModel.SearchRequestTypeId > 0 ? (GdprRequestType?)searchModel.SearchRequestTypeId : null,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = new GdprLogListModel
            {
                Data = gdprLog.Select(log =>
                {
                    //fill in model values from the entity
                    var customer = _customerService.GetCustomerById(log.CustomerId);

                    var requestModel = new GdprLogModel
                    {
                        Id = log.Id,
                        CustomerInfo = customer != null && !customer.Deleted && !String.IsNullOrEmpty(customer.Email) ?
                            customer.Email :
                            log.CustomerInfo,
                        RequestType = _localizationService.GetLocalizedEnum(log.RequestType),
                        RequestDetails = log.RequestDetails,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc)
                    };
                    return requestModel;
                }),
                Total = gdprLog.TotalCount
            };

            return model;
        }

        #endregion
    }
}