using System.Collections.Generic;
using Nop.Core.Domain.Security;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer role
    /// </summary>
    public partial class CustomerRole : BaseEntity
    {
        private ICollection<PermissionRecordCustomerRoleMapping> _permissionRecordCustomerRoleMappings;

        /// <summary>
        /// Gets or sets the customer role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is system
        /// </summary>
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// Gets or sets the customer role system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customers must change passwords after a specified time
        /// </summary>
        public bool EnablePasswordLifetime { get; set; }

        /// <summary>
        /// Gets or sets the permission record-customer role mappings
        /// </summary>
        public virtual ICollection<PermissionRecordCustomerRoleMapping> PermissionRecordCustomerRoleMappings
        {
            get => _permissionRecordCustomerRoleMappings ?? (_permissionRecordCustomerRoleMappings = new List<PermissionRecordCustomerRoleMapping>());
            protected set => _permissionRecordCustomerRoleMappings = value;
        }
    }
}