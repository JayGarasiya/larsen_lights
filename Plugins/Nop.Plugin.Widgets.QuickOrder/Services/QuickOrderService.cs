using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Cms;

namespace Nop.Plugin.Widgets.QuickOrder.Services;

/// <summary>
/// Quick order service implementation.
/// Provides business logic related to quick orders and customer retrieval.
/// </summary>
public partial class QuickOrderService : IQuickOrderService
{
    #region Fields

    protected readonly IRepository<Customer> _customerRepository;
    protected readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IWidgetPluginManager _widgetPluginManager;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public QuickOrderService(
        IRepository<Customer> customerRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<Order> orderRepository,
        IWidgetPluginManager widgetPluginManager,
        IWorkContext workContext)
    {
        _customerRepository = customerRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _orderRepository = orderRepository;
        _widgetPluginManager = widgetPluginManager;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks whether the QuickOrder plugin is active for the current customer and store.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a boolean indicating if the plugin is active.
    /// </returns>
    public async Task<bool> PluginActiveAsync()
    {
        return await _widgetPluginManager.IsPluginActiveAsync("Widgets.QuickOrder", await _workContext.GetCurrentCustomerAsync());
    }

    /// <summary>
    /// Retrieves customers based on various filter criteria (e.g., registration date, activity, roles, etc.).
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
    /// The task result contains a paged list of customers matching the filter criteria.
    /// </returns>
    public virtual async Task<IPagedList<Customer>> GetAllCustomersAsync(
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null,
        DateTime? lastActivityFromUtc = null,
        DateTime? lastActivityToUtc = null,
        int affiliateId = 0,
        int vendorId = 0,
        int[] customerRoleIds = null,
        string email = null,
        string username = null,
        string firstName = null,
        string lastName = null,
        int dayOfBirth = 0,
        int monthOfBirth = 0,
        string company = null,
        string phone = null,
        string zipPostalCode = null,
        string ipAddress = null,
        bool? isActive = null,
        string customOrderNumber = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false)
    {
        // Query customers from the repository with filters applied
        var customers = await _customerRepository.GetAllPagedAsync(query =>
        {
            // Apply filters based on the input parameters
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
            if (lastActivityFromUtc.HasValue)
                query = query.Where(c => lastActivityFromUtc.Value <= c.LastActivityDateUtc);
            if (lastActivityToUtc.HasValue)
                query = query.Where(c => lastActivityToUtc.Value >= c.LastActivityDateUtc);
            if (affiliateId > 0)
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (vendorId > 0)
                query = query.Where(c => vendorId == c.VendorId);
            if (isActive.HasValue)
                query = query.Where(c => c.Active == isActive.Value);
            // Exclude deleted customers
            query = query.Where(c => !c.Deleted);

            // Filter by customer roles if provided
            if (customerRoleIds != null && customerRoleIds.Length > 0)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                        (x, y) => new { Customer = x, Mapping = y })
                    .Where(z => customerRoleIds.Contains(z.Mapping.CustomerRoleId))
                    .Select(z => z.Customer)
                    .Distinct();
            }

            // Apply additional filters (email, username, etc.)
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(c => c.FirstName.Contains(firstName));
            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(c => c.LastName.Contains(lastName));
            if (!string.IsNullOrWhiteSpace(company))
                query = query.Where(c => c.Company.Contains(company));
            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.Phone.Contains(phone));
            if (!string.IsNullOrWhiteSpace(zipPostalCode))
                query = query.Where(c => c.ZipPostalCode.Contains(zipPostalCode));

            // Handle birth date filtering
            if (dayOfBirth > 0 && monthOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Day == dayOfBirth &&
                    c.DateOfBirth.Value.Month == monthOfBirth);
            else if (dayOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Day == dayOfBirth);
            else if (monthOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Month == monthOfBirth);

            // Filter by IP address if provided and valid
            if (!string.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.LastIpAddress == ipAddress);
            }

            // Filter by custom order number if provided
            if (!string.IsNullOrWhiteSpace(customOrderNumber))
            {
                query = query.Join(_orderRepository.Table, x => x.Id, y => y.CustomerId,
                        (x, y) => new { Customer = x, Order = y })
                    .Where(z => z.Order.CustomOrderNumber.Equals(customOrderNumber))
                    .Select(z => z.Customer)
                    .Distinct();
            }

            // Order customers by creation date
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return customers;
    }

    #endregion
}
