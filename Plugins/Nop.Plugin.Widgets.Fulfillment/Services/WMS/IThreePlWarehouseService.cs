using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Inventory;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders;

namespace Nop.Plugin.Widgets.Fulfillment.Services.WMS
{
    /// <summary>
    /// Represents service 3PL central warehouse service interface
    /// </summary>
    public partial interface IThreePlWarehouseService
    {
        #region Authentication

        /// <summary>
        /// Get access token
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="clientSecret">Client Secret</param>
        /// <param name="threePlKey">3PL Key</param>
        /// <param name="userLoginId">User Login Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the access token of the 3PL central
        /// </returns>
        Task<string> GetAuthenticationToken(string clientId, string clientSecret, string threePlKey, string userLoginId);

        #endregion

        #region Order

        /// <summary>
        /// Check product warehouse change for order
        /// </summary>
        /// <param name="orderId">Order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task CheckProductWarehouseChange(int orderId);

        /// <summary>
        /// Create an order to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="order">Order dto object</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order result of the 3PL central
        /// </returns>
        Task<OrderResultDto> CreateOrder(string accessToken, OrderDto order);

        /// <summary>
        /// Cancel an order to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">order identifier</param>
        /// <param name="order">Order cancel dto object</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result of the 3PL central
        /// </returns>
        Task<bool> CancelOrder(string accessToken, int orderId, OrderCancelDto order);

        /// <summary>
        /// Get an order detail from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order of the 3PL central
        /// </returns>
        Task<OrderResultDto> GetOrderDetails(string accessToken, int orderId);

        /// <summary>
        /// Get an order items detail from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order items of the 3PL central
        /// </returns>
        Task<OrderItemsResultDto> GetOrderItemsDetails(string accessToken, int orderId);

        /// <summary>
        /// Sync an orders to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orders">Order identifiers</param>
        /// <param name="manualSync">Whether it's a force to synchronization manually</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task<IList<int>> SyncOrders(string accessToken, List<ThreePlRecord> orders, bool manualSync = false);

        /// <summary>
        /// Sync cancel orders to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orders">Order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task SyncCancelOrders(string accessToken, List<ThreePlRecord> orders);

        /// <summary>
        /// Validate product
        /// </summary>
        /// <param name="orderItem">OrderItem object</param>
        /// <param name="customer">Customer object</param>
        /// <param name="store">Store object</param>
        /// <param name="impersonatedOrder">Impersonated order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the instock, outstock and error note of the 3PL central
        /// </returns>
        Task<SyncOrderItem> ValidateProductAsync(OrderItem orderItem, Customer customer, Store store, bool impersonatedOrder = false);

        #endregion

        #region Inventory

        /// <summary>
        /// Get product stock summary from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="page">page number</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the inventory stock result of the 3PL central
        /// </returns>
        Task<StockSummaryDto> GetItemStockSummary(string accessToken, string page);

        #endregion
    }
}
