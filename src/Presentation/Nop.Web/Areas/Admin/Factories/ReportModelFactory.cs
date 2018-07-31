using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Extensions;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the report model factory implementation
    /// </summary>
    public partial class ReportModelFactory : IReportModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerReportService _customerReportService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ReportModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerReportService customerReportService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IProductService productService,
            IWorkContext workContext)
        {
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._countryService = countryService;
            this._customerReportService = customerReportService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._productService = productService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        #region Customer reports

        /// <summary>
        /// Prepare customer reports search model
        /// </summary>
        /// <param name="searchModel">Customer reports search model</param>
        /// <returns>Customer reports search model</returns>
        public virtual CustomerReportsSearchModel PrepareCustomerReportsSearchModel(CustomerReportsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare nested search models
            PrepareRegisteredCustomersReportSearchModel(searchModel.RegisteredCustomers);

            return searchModel;
        }


        /// <summary>
        /// Prepare registered customers report search model
        /// </summary>
        /// <param name="searchModel">Registered customers report search model</param>
        /// <returns>Registered customers report search model</returns>
        protected virtual RegisteredCustomersReportSearchModel PrepareRegisteredCustomersReportSearchModel(RegisteredCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged registered customers report list model
        /// </summary>
        /// <param name="searchModel">Registered customers report search model</param>
        /// <returns>Registered customers report list model</returns>
        public virtual RegisteredCustomersReportListModel PrepareRegisteredCustomersReportListModel(RegisteredCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get report items
            var reportItems = new List<RegisteredCustomersReportModel>
            {
                new RegisteredCustomersReportModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.7days"),
                    Customers = _customerReportService.GetRegisteredCustomersReport(7)
                },
                new RegisteredCustomersReportModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.14days"),
                    Customers = _customerReportService.GetRegisteredCustomersReport(14)
                },
                new RegisteredCustomersReportModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.month"),
                    Customers = _customerReportService.GetRegisteredCustomersReport(30)
                },
                new RegisteredCustomersReportModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.year"),
                    Customers = _customerReportService.GetRegisteredCustomersReport(365)
                }
            };

            //prepare list model
            var model = new RegisteredCustomersReportListModel
            {
                Data = reportItems.PaginationByRequestModel(searchModel),
                Total = reportItems.Count
            };

            return model;
        }

        #endregion

        #endregion
    }
}