using System.Collections.Generic;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Validators.Customers;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer role model
    /// </summary>
    [Validator(typeof(CustomerRoleValidator))]
    public partial class CustomerRoleModel : BaseNopEntityModel
    {
        #region Ctor

        public CustomerRoleModel()
        {
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.EnablePasswordLifetime")]
        public bool EnablePasswordLifetime { get; set; }

        #endregion
    }
}