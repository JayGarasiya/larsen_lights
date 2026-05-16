using Nop.Core;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Widgets.QuickOrder.Services;

/// <summary>
/// Interface for the quick order service
/// Provides methods related to checking plugin activity and customer data retrieval
/// </summary>
public partial interface IQuickOrderService
{
    /// <summary>
    /// Checks whether the plugin is active for the current customer and store.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the status indicating whether the plugin is active.
    /// </returns>
    Task<bool> PluginActiveAsync();

    /// <summary>
    /// Retrieves all customers based on various filter criteria.
    /// </summary>
    /// <param name="createdFromUtc">Filter customers created from this date (UTC); null to load all records.</param>
    /// <param name="createdToUtc">Filter customers created to this date (UTC); null to load all records.</param>
    /// <param name="lastActivityFromUtc">Filter customers with last activity from this date (UTC); null to load all records.</param>
    /// <param name="lastActivityToUtc">Filter customers with last activity to this date (UTC); null to load all records.</param>
    /// <param name="affiliateId">Filter by affiliate identifier.</param>
    /// <param name="vendorId">Filter by vendor identifier.</param>
    /// <param name="customerRoleIds">Filter by customer roles; pass null or empty list to load all customers.</param>
    /// <param name="email">Filter by email; null to load all customers.</param>
    /// <param name="username">Filter by username; null to load all customers.</param>
    /// <param name="firstName">Filter by first name; null to load all customers.</param>
    /// <param name="lastName">Filter by last name; null to load all customers.</param>
    /// <param name="dayOfBirth">Filter by day of birth; 0 to load all customers.</param>
    /// <param name="monthOfBirth">Filter by month of birth; 0 to load all customers.</param>
    /// <param name="company">Filter by company; null to load all customers.</param>
    /// <param name="phone">Filter by phone number; null to load all customers.</param>
    /// <param name="zipPostalCode">Filter by zip/postal code; null to load all customers.</param>
    /// <param name="ipAddress">Filter by IP address; null to load all customers.</param>
    /// <param name="customOrderNumber">Filter by custom order number; null to load all customers.</param>
    /// <param name="pageIndex">Page index for pagination.</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="getOnlyTotalCount">Indicates whether to load only the total count of records (without data).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a paged list of customers matching the criteria.
    /// </returns>
    Task<IPagedList<Customer>> GetAllCustomersAsync(
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null, DateTime? lastActivityFromUtc = null,
        DateTime? lastActivityToUtc = null, int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null,
        string email = null, string username = null, string firstName = null, string lastName = null, int dayOfBirth = 0,
        int monthOfBirth = 0, string company = null, string phone = null, string zipPostalCode = null,
        string ipAddress = null, bool? isActive = null, string customOrderNumber = null, int pageIndex = 0, int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false);
}
