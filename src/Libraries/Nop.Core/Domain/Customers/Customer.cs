using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Common;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class Customer : BaseEntity
    {
        private ICollection<ExternalAuthenticationRecord> _externalAuthenticationRecords;
        private ICollection<CustomerCustomerRoleMapping> _customerCustomerRoleMappings;
        protected ICollection<CustomerAddressMapping> _customerAddressMappings;
        private IList<CustomerRole> _customerRoles;

        public Customer()
        {
            this.CustomerGuid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the customer GUID
        /// </summary>
        public Guid CustomerGuid { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the email that should be re-validated. Used in scenarios when a customer is already registered and wants to change an email address.
        /// </summary>
        public string EmailToRevalidate { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier with which this customer is associated (maganer)
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is required to re-login
        /// </summary>
        public bool RequireReLogin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of failed login attempts (wrong password)
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the date and time until which a customer cannot login (locked out)
        /// </summary>
        public DateTime? CannotLoginUntilDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Gets or sets the customer system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        /// <summary>
        ///  Gets or sets the store identifier in which customer registered
        /// </summary>
        public int RegisteredInStoreId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Gets or sets customer generated content
        /// </summary>
        public virtual ICollection<ExternalAuthenticationRecord> ExternalAuthenticationRecords
        {
            get => _externalAuthenticationRecords ?? (_externalAuthenticationRecords = new List<ExternalAuthenticationRecord>());
            protected set => _externalAuthenticationRecords = value;
        }

        /// <summary>
        /// Gets or sets customer roles
        /// </summary>
        public virtual IList<CustomerRole> CustomerRoles
        {
            get => _customerRoles ?? (_customerRoles = CustomerCustomerRoleMappings.Select(mapping => mapping.CustomerRole).ToList());
        }

        /// <summary>
        /// Gets or sets customer-customer role mappings
        /// </summary>
        public virtual ICollection<CustomerCustomerRoleMapping> CustomerCustomerRoleMappings
        {
            get => _customerCustomerRoleMappings ?? (_customerCustomerRoleMappings = new List<CustomerCustomerRoleMapping>());
            protected set => _customerCustomerRoleMappings = value;
        }

        /// <summary>
        /// Gets or sets customer addresses
        /// </summary>
        public IList<Address> Addresses => CustomerAddressMappings.Select(mapping => mapping.Address).ToList();

        /// <summary>
        /// Gets or sets customer-address mappings
        /// </summary>
        public virtual ICollection<CustomerAddressMapping> CustomerAddressMappings
        {
            get => _customerAddressMappings ?? (_customerAddressMappings = new List<CustomerAddressMapping>());
            protected set => _customerAddressMappings = value;
        }

        #endregion
    }
}