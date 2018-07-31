using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Web.Areas.Admin.Models.Customers;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial interface ICustomerModelFactory
    {
        /// <summary>
        /// Prepare customer search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer search model</returns>
        CustomerSearchModel PrepareCustomerSearchModel(CustomerSearchModel searchModel);

        /// <summary>
        /// Prepare paged customer list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer list model</returns>
        CustomerListModel PrepareCustomerListModel(CustomerSearchModel searchModel);

        /// <summary>
        /// Prepare customer model
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer model</returns>
        CustomerModel PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties = false);

        /// <summary>
        /// Prepare paged customer address list model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address list model</returns>
        CustomerAddressListModel PrepareCustomerAddressListModel(CustomerAddressSearchModel searchModel, Customer customer);

        /// <summary>
        /// Prepare customer address model
        /// </summary>
        /// <param name="model">Customer address model</param>
        /// <param name="customer">Customer</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer address model</returns>
        CustomerAddressModel PrepareCustomerAddressModel(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false);

        /// <summary>
        /// Prepare paged customer activity log list model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log list model</returns>
        CustomerActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel, Customer customer);
        
        /// <summary>
        /// Prepare online customer search model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer search model</returns>
        OnlineCustomerSearchModel PrepareOnlineCustomerSearchModel(OnlineCustomerSearchModel searchModel);

        /// <summary>
        /// Prepare paged online customer list model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer list model</returns>
        OnlineCustomerListModel PrepareOnlineCustomerListModel(OnlineCustomerSearchModel searchModel);

        /// <summary>
        /// Prepare GDPR request (log) search model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request search model</returns>
        GdprLogSearchModel PrepareGdprLogSearchModel(GdprLogSearchModel searchModel);

        /// <summary>
        /// Prepare paged GDPR request list model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request list model</returns>
        GdprLogListModel PrepareGdprLogListModel(GdprLogSearchModel searchModel);

    }
}