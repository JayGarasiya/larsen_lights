using Nop.Core;
using Nop.Plugin.Widgets.Fulfillment.Domain;

namespace Nop.Plugin.Widgets.Fulfillment.Services.ThreePl
{
    /// <summary>
    /// Represents service 3PL central service interface
    /// </summary>
    public partial interface IThreePlService
    {
        /// <summary>
        /// Check whether the plugin is active for the current user and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<bool> PluginActiveAsync();

        #region 3PL Record

        /// <summary>
        /// Get a 3PL central record by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central record
        /// </returns>
        Task<ThreePlRecord> GetThreePlRecordByOrderIdAsync(int orderId);

        /// <summary>
        /// Get a 3PL central record by identifier
        /// </summary>
        /// <param name="threePlRecordId">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central record
        /// </returns>
        Task<ThreePlRecord> GetThreePlRecordByIdAsync(int threePlRecordId);

        /// <summary>
        /// Insert the 3PL central record
        /// </summary>
        /// <param name="threePlRecord">3PL central record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertThreePlRecordAsync(ThreePlRecord threePlRecord);

        /// <summary>
        /// Search 3PL records
        /// </summary>
        /// <param name="orderId">Order identifier; 0 to load all 3PL records</param>
        /// <param name="threePlOrderId">3PL order identifier; null to load all 3PL records</param>
        /// <param name="tsIds">3PL status identifiers; null to load all 3PL records</param>
        /// <param name="etsIds">Exclude 3PL status identifiers; null to load all 3PL records</param>
        /// <param name="onlyCheckMoneyOrder">Only paid by check money order. 0 to load all 3PL records</param>
        /// <param name="onlyCancelledOrder">Only cancelled  order. 0 to load all 3PL records</param>
        /// <param name="excludeCancelledOrder">Exclude cancelled  order. 0 to load all 3PL records</param>
        /// <param name="excludeDeletedOrder">Exclude deleted order; 0 to load all 3PL records</param>
        /// <param name="cancelledDate">Cancelled date to (UTC); null to load all 3PL records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL records
        /// </returns>
        Task<IPagedList<ThreePlRecord>> SearchThreePlRecordsAsync(int orderId = 0, int threePlOrderId = 0, List<int> tsIds = null, List<int> etsIds = null,
            bool onlyCheckMoneyOrder = false, bool onlyCancelledOrder = false, bool excludeCancelledOrder = false, bool excludeDeletedOrder = false,
            DateTime? cancelledDate = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Update the 3PL central record
        /// </summary>
        /// <param name="threePlRecord">3PL central record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateThreePlRecordAsync(ThreePlRecord threePlRecord);

        #endregion

        #region 3PL Order

        /// <summary>
        /// Get a 3PL central Order by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central Order 
        /// </returns>
        Task<IList<ThreePlOrder>> GetThreePlOrdersByOrderIdAsync(int orderId);

        /// <summary>
        /// Get a 3PL central Order by identifier
        /// </summary>
        /// <param name="threePlOrderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central Order
        /// </returns>
        Task<ThreePlOrder> GetThreePlOrderByIdAsync(int threePlOrderId);

        /// <summary>
        /// Insert the 3PL central Order
        /// </summary>
        /// <param name="threePlOrder">3PL central Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertThreePlOrderAsync(ThreePlOrder threePlOrder);

        /// <summary>
        /// Search 3PL orders
        /// </summary>
        /// <param name="threePlId">3Pl record identifier; 0 to load all 3PL orders</param>
        /// <param name="orderId">Order identifier; 0 to load all 3PL orders</param>
        /// <param name="threePlOrderId">3PL order identifier; null to load all 3PL orders</param>
        /// <param name="tsIds">3PL status identifiers; null to load all 3PL orders</param>
        /// <param name="onlyCancelledOrder">Only cancelled  order. 0 to load all 3PL orders</param>
        /// <param name="excludeCancelledOrder">Exclude cancelled  order. 0 to load all 3PL orders</param>
        /// <param name="excludeDeletedOrder">Exclude deleted order; 0 to load all 3PL orders</param>
        /// <param name="cancelledDate">Cancelled date to (UTC); null to load all 3PL orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL orders
        /// </returns>
        Task<IPagedList<ThreePlOrder>> SearchThreePlOrdersAsync(int threePlId = 0, int orderId = 0, int threePlOrderId = 0, List<int> tsIds = null, List<int> etsIds = null,
            bool onlyCancelledOrder = false, bool excludeCancelledOrder = false, bool excludeDeletedOrder = false,
            DateTime? cancelledDate = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Update the 3PL central Order
        /// </summary>
        /// <param name="threePlOrder">3PL central Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateThreePlOrderAsync(ThreePlOrder threePlOrder);

        #endregion

        #region 3PL Shipping Method

        /// <summary>
        /// Delete a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod);

        /// <summary>
        /// Gets all 3PL Shipping Methods
        /// </summary>
        /// <param name="shippingMethod">Shipping method; null to load all 3PL orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax 3PL Shipping Method
        /// </returns>
        Task<IPagedList<ThreePlShippingMethod>> GetAllThreePlShippingMethodsAsync(string shippingMethod, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethodId">3PL Shipping Method identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL Shipping Method
        /// </returns>
        Task<ThreePlShippingMethod> GetThreePlShippingMethodByIdAsync(int threePlShippingMethodId);

        /// <summary>
        /// Gets a 3PL Shipping Method
        /// </summary>
        /// <param name="shippingMethod">Default Shipping Method identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL Shipping Method
        /// </returns>
        Task<ThreePlShippingMethod> GetThreePlShippingMethodByShippingMethodAsync(string shippingMethod);

        /// <summary>
        /// Insert a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod);

        /// <summary>
        /// Update the 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod);

        #endregion
    }
}
