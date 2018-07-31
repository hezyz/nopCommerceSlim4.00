using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Helpers;
using System;
using System.Linq;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer report service
    /// </summary>
    public partial class CustomerReportService : ICustomerReportService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<Customer> _customerRepository;

        #endregion

        #region Ctor

        public CustomerReportService(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IRepository<Customer> customerRepository)
        {
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerRepository = customerRepository;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>Number of registered customers</returns>
        public virtual int GetRegisteredCustomersReport(int days)
        {
            var date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-days);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
            if (registeredCustomerRole == null)
                return 0;

            var query = from c in _customerRepository.Table
                        from mapping in c.CustomerCustomerRoleMappings
                        where !c.Deleted &&
                        mapping.CustomerRoleId == registeredCustomerRole.Id &&
                        c.CreatedOnUtc >= date
                        //&& c.CreatedOnUtc <= DateTime.UtcNow
                        select c;

            var count = query.Count();
            return count;
        }

        #endregion
    }
}