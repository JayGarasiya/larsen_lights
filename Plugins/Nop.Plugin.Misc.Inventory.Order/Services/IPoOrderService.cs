using Nop.Core;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Services
{
    /// <summary>
    /// Represents the interface po order service
    /// </summary>
    public partial interface IPoOrderService
    {
        #region PoOrder Method
        /// <summary>
        /// Retrieve a paged list of purchase orders based on filter criteria
        /// </summary>
        /// <param name="poNumber">Purchase order number</param>
        /// <param name="adminComment">Admin comment filter</param>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to include hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged list of purchase orders
        /// </returns>
        Task<IPagedList<PoOrder>> GetAllPoOrder(string poNumber = "", string adminComment = "", DateTime? startDate = null, DateTime? endDate = null, int pageIndex = 0, int pageSize = 2147483647, bool showHidden = false);

        /// <summary>
        /// Insert a new purchase order
        /// </summary>
        /// <param name="poOrder">Purchase order entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertPoOrder(PoOrder poOrder);

        /// <summary>
        /// Update an existing purchase order
        /// </summary>
        /// <param name="poOrder">Purchase order entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdatePoOrder(PoOrder poOrder);

        /// <summary>
        /// Get a purchase order by identifier
        /// </summary>
        /// <param name="id">Purchase order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order
        /// </returns>
        Task<PoOrder> GetPoOrderById(int id);

        /// <summary>
        /// Get multiple purchase orders by identifiers
        /// </summary>
        /// <param name="poOrderIds">Array of purchase order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the list of purchase orders
        /// </returns>
        Task<IList<PoOrder>> GetPoOrderByIdsAsync(int[] poOrderIds);
        #endregion

        #region PoOrderItem Method
        /// <summary>
        /// Retrieve purchase order items for a specific purchase order
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to include hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged list of purchase order items
        /// </returns>
        Task<IPagedList<PoOrderItem>> GetPoOrderItemsByPoOrderAsync(int poOrderId = 0, int pageIndex = 0, int pageSize = 2147483647, bool showHidden = false);

        /// <summary>
        /// Get a purchase order item by identifier
        /// </summary>
        /// <param name="id">Purchase order item identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        Task<PoOrderItem> GetByIdPoOrderItem(int id);

        /// <summary>
        /// Insert a new purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertPoOrderItem(PoOrderItem poOrderItem);

        /// <summary>
        /// Update an existing purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdatePoOrderitem(PoOrderItem poOrderItem);

        /// <summary>
        /// Delete a purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeletePoOrderitem(PoOrderItem poOrderItem);

        /// <summary>
        /// Get a purchase order item by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        Task<PoOrderItem> GetPoOrderItemByProductId(int productId);
        #endregion

        #region ProductDimension Method
        /// <summary>
        /// Insert product dimension details
        /// </summary>
        /// <param name="productDimensions">Product dimension entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductDimensions(ProductDimensions productDimensions);

        /// <summary>
        /// Update product dimension details
        /// </summary>
        /// <param name="productDimensions">Product dimension entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductDimensions(ProductDimensions productDimensions);

        /// <summary>
        /// Get product dimensions by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the product dimensions
        /// </returns>
        Task<ProductDimensions> GetProductDimensionsByProductId(int productId);

        /// <summary>
        /// Get a purchase order item by purchase order and product identifiers
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        Task<PoOrderItem> GetPoOrderItemByPoOrderIdAndProductId(int poOrderId, int productId);
        #endregion

        #region Inventory Search
        /// <summary>
        /// Prepare the manage order report model for inventory analysis
        /// </summary>
        /// <param name="startDateValue">Report start date</param>
        /// <param name="endDateValue">Report end date</param>
        /// <param name="searchManufacturerId">Manufacturer filter</param>
        /// <param name="searchCategoryId">Category filter</param>
        /// <param name="searchPercentage">Percentage filter</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged manage order report model
        /// </returns>
        Task<IPagedList<MangeOrderReportModel>> PrepareMangeOrderReportModel(DateTime? startDateValue, DateTime? endDateValue, int? searchManufacturerId, int? searchCategoryId, int? searchPercentage, int pageIndex = 0, int pageSize = int.MaxValue);
        #endregion

        #region OrderAssociatedProductMap
        /// <summary>
        /// Insert order associated product mapping
        /// </summary>
        /// <param name="orderAssociatedProductMap">Order associated product map entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertOrderAssociatedProductMap(OrderAssociatedProductMap orderAssociatedProductMap);

        /// <summary>
        /// Delete order associated product mappings by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteOrderAssociatedProductMaps(int orderId);

        /// <summary>
        /// Delete a purchase order by identifier
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeletePoOrder(int poOrderId);
        #endregion
    }
}
