using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Reports;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ReportController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IReportModelFactory _reportModelFactory;

        #endregion

        #region Ctor

        public ReportController(
            IPermissionService permissionService,
            IReportModelFactory reportModelFactory)
        {
            this._permissionService = permissionService;
            this._reportModelFactory = reportModelFactory;
        }

        #endregion

        #region Methods

        #region Customer reports

        public virtual IActionResult Customers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareCustomerReportsSearchModel(new CustomerReportsSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ReportRegisteredCustomersList(RegisteredCustomersReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _reportModelFactory.PrepareRegisteredCustomersReportListModel(searchModel);

            return Json(model);
        }        

        #endregion

        #endregion
    }
}
