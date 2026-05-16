using Nop.Plugin.Widgets.QuickOrder.Models;
using Nop.Web.Areas.Admin.Models.Customers;

namespace Nop.Plugin.Widgets.QuickOrder.Factories;

/// <summary>
/// Factory to prepare models related to Quick Order functionality.
/// </summary>
public interface IQuickOrderModelFactory
{
    /// <summary>
    /// Prepare the customer search model with necessary settings.
    /// </summary>
    /// <param name="searchModel">The customer search model to be populated.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The result contains the updated search model.
    /// </returns>
    Task<QuickOrderSearchModel> PrepareCustomerSearchModelAsync(QuickOrderSearchModel searchModel);

    /// <summary>
    /// Prepare a paged customer list model based on the search parameters.
    /// </summary>
    /// <param name="searchModel">The customer search model containing filter criteria.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The result contains the customer list model.
    /// </returns>
    Task<CustomerListModel> PrepareCustomerListModelAsync(QuickOrderSearchModel searchModel);
}
