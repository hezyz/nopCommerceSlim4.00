using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Home;
using Nop.Web.Framework.Components;

namespace Nop.Web.Areas.Admin.Components
{
    public class CommonStatisticsViewComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public CommonStatisticsViewComponent(IPermissionService permissionService,
            IProductService productService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            this._permissionService = permissionService;
            this._productService = productService;
            this._customerService = customerService;
            this._workContext = workContext;
        }

        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("");

            var model = new CommonStatisticsModel
            {
                NumberOfCustomers = _customerService.GetAllCustomers(
                customerRoleIds: new[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id },
                pageIndex: 0,
                pageSize: 1).TotalCount,
            };

            return View(model);
        }
    }
}
