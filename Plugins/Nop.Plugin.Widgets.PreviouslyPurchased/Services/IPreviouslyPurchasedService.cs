using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Services;

/// <summary>
/// Previously purchased service interface
/// </summary>
public partial interface IPreviouslyPurchasedService
{
    /// <summary>
    /// Checks whether the 'Previously Purchased' plugin is active for the current customer and store.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result indicates whether the plugin is active.
    /// </returns>
    Task<bool> PluginActiveAsync();

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
    Task<IPagedList<Product>> PreviouslyPurchasedReportAsync(int customerId, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, ProductSortingEnum orderBy = ProductSortingEnum.Position, bool showHidden = false);
}
