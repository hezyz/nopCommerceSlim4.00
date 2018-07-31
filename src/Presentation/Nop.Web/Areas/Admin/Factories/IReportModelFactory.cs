using Nop.Web.Areas.Admin.Models.Reports;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the report model factory
    /// </summary>
    public partial interface IReportModelFactory
    {
        #region Customer reports

        /// <summary>
        /// Prepare customer reports search model
        /// </summary>
        /// <param name="searchModel">Customer reports search model</param>
        /// <returns>Customer reports search model</returns>
        CustomerReportsSearchModel PrepareCustomerReportsSearchModel(CustomerReportsSearchModel searchModel);

        /// <summary>
        /// Prepare paged registered customers report list model
        /// </summary>
        /// <param name="searchModel">Registered customers report search model</param>
        /// <returns>Registered customers report list model</returns>
        RegisteredCustomersReportListModel PrepareRegisteredCustomersReportListModel(RegisteredCustomersReportSearchModel searchModel);

        #endregion
    }
}
