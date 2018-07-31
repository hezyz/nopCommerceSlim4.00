using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a customer reports search model
    /// </summary>
    public partial class CustomerReportsSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomerReportsSearchModel()
        {
            RegisteredCustomers = new RegisteredCustomersReportSearchModel();
        }

        #endregion

        #region Properties

        public RegisteredCustomersReportSearchModel RegisteredCustomers { get; set; }

        #endregion
    }
}