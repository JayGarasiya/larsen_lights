using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Widgets.PreviouslyPurchased.Domain;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Services;

/// <summary>
/// Provides services related to previously purchased products.
/// </summary>
public partial class PreviouslyPurchasedService : IPreviouslyPurchasedService
{
    #region Fields
    private readonly PreviouslyPurchasedSettings _previouslyPurchasedSettings;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IWorkContext _workContext;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IAclService _aclService;
    private readonly IProductService _productService;
    private readonly IStoreMappingService _storeMappingService;
    #endregion

    #region Ctor
    public PreviouslyPurchasedService(
        PreviouslyPurchasedSettings previouslyPurchasedSettings,
        IWidgetPluginManager widgetPluginManager,
        IWorkContext workContext,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IAclService aclService,
        IProductService productService,
        IStoreMappingService storeMappingService)
    {
        _previouslyPurchasedSettings = previouslyPurchasedSettings;
        _widgetPluginManager = widgetPluginManager;
        _workContext = workContext;
        _orderItemRepository = orderItemRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _aclService = aclService;
        _productService = productService;
        _storeMappingService = storeMappingService;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Searches for order items based on specified criteria such as store, customer, and visibility.
    /// </summary>
    /// <param name="storeId">Store identifier (0 to load all stores)</param>
    /// <param name="customerId">Customer identifier (0 to load all customers)</param>
    /// <param name="showHidden">Indicates whether to include hidden products</param>
    /// <returns>Queryable collection of order items</returns>
    private IQueryable<OrderItem> SearchOrderItems(
        int storeId = 0,
        int customerId = 0,
        bool showHidden = false)
    {
        var orderItems = from orderItem in _orderItemRepository.Table
                         join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
                         where (storeId == 0 || storeId == o.StoreId)
                               && !o.Deleted
                               && !p.Deleted
                               && o.CustomerId == customerId
                               && (showHidden || p.Published)
                         select orderItem;

        return orderItems;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks whether the 'Previously Purchased' plugin is active for the current customer and store.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result indicates whether the plugin is active.
    /// </returns>
    public async Task<bool> PluginActiveAsync()
    {
        return await _widgetPluginManager.IsPluginActiveAsync(PreviouslyPurchasedDefaults.SystemName, await _workContext.GetCurrentCustomerAsync());
    }

    /// <summary>
    /// Retrieves a list of previously purchased products for a given customer.
    /// </summary>
    /// <param name="customerId">The ID of the customer</param>
    /// <param name="storeId">The ID of the store (0 for all stores)</param>
    /// <param name="pageIndex">The page index for pagination</param>
    /// <param name="pageSize">The number of records per page</param>
    /// <param name="orderBy">The sorting order for the products</param>
    /// <param name="showHidden">Indicates whether to show hidden products</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a list of previously purchased products.
    /// </returns>
    public virtual async Task<IPagedList<Product>> PreviouslyPurchasedReportAsync(
        int customerId,
        int storeId = 0,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false)
    {
        // Check if the plugin is active; if not, return null
        if (!await PluginActiveAsync())
            return null;

        // Search order items for the specified customer and store
        var previouslyPurchased = SearchOrderItems(storeId, customerId, showHidden);

        // Group order items by product ID and select the min and max order IDs
        var ppReport =
            from orderItem in previouslyPurchased
            group orderItem by orderItem.ProductId into g
            select new
            {
                ProductId = g.Key,
                OrderIdMax = g.Max(x => x.OrderId),
                OrderIdMin = g.Min(x => x.OrderId)
            };

        // Sort the report based on product type (first purchase, last purchase, or random)
        ppReport = _previouslyPurchasedSettings.ProductsType switch
        {
            (int)PreviouslyPurchasedType.FirstPurchase => ppReport.OrderByDescending(x => x.OrderIdMin),
            (int)PreviouslyPurchasedType.LastPurchase => ppReport.OrderByDescending(x => x.OrderIdMax),
            (int)PreviouslyPurchasedType.RandomNumber => ppReport.OrderBy(x => Guid.NewGuid()),
            _ => throw new ArgumentException("Invalid ProductsType setting", nameof(_previouslyPurchasedSettings.ProductsType))
        };

        // Get the product IDs
        var pIds = ppReport.Select(p => p.ProductId);

        // Retrieve the products based on the IDs, and apply ACL, store mapping, and availability checks
        var products = await (await _productService.GetProductsByIdsAsync(pIds.ToArray()))
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            .Where(p => _productService.ProductIsAvailable(p)) // Check product availability
            .ToListAsync();

        // Apply sorting to the products
        var productsQuery = products.AsQueryable();
        if (orderBy != ProductSortingEnum.Position)
        {
            productsQuery = orderBy switch
            {
                ProductSortingEnum.NameAsc => productsQuery.OrderBy(p => p.Name),
                ProductSortingEnum.NameDesc => productsQuery.OrderByDescending(p => p.Name),
                ProductSortingEnum.PriceAsc => productsQuery.OrderBy(p => p.Price),
                ProductSortingEnum.PriceDesc => productsQuery.OrderByDescending(p => p.Price),
                ProductSortingEnum.CreatedOn => productsQuery.OrderByDescending(p => p.CreatedOnUtc),
                ProductSortingEnum.Position when productsQuery is IOrderedQueryable => productsQuery,
                _ => productsQuery.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
            };
        }

        // Return the paginated list of products
        return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
    }
    #endregion
}
