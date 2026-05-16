using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Factories
{
    /// <summary>
    /// Represents the interface of the inventory order factory
    /// </summary>
    public partial interface IInventoryOrderFactory
    {
        #region Po Order
        /// <summary>
        /// Prepare the manage inventory search model
        /// </summary>
        /// <param name="searchModel">Manage Inventory Search Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manage inventory search model
        /// </returns>
        Task<ManageInventoryListModel> PrepareManageInventorySearchModel(ManageInventorySearchModel searchModel);

        /// <summary>
        /// Prepare the manage inventory list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manage inventory list model
        /// </returns>
        Task<ManageInventorySearchModel> PrepareManageInventoryListModel();
        #endregion

        #region PoOrder Item
        /// <summary>
        /// Prepare the po order search model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order search model
        /// </returns>
        Task<PoOrderSearchModel> PreparePoOrderSearchModel(PoOrderSearchModel searchModel);

        /// <summary>
        /// Prepare the po order list model
        /// </summary>
        /// <param name="searchModel">Po Order Item Search Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order list model
        /// </returns>
        Task<PoOrderListModel> PreparePoOrderListModel(PoOrderSearchModel searchModel);

        /// <summary>
        /// Prepare the po order item by po number
        /// </summary>
        /// <param name="searchModel">po order item search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order item list model
        /// </returns>
        Task<PoOrderItemListModel> PreparePoOrderItemByPoNumber(PoOrderItemSearchModel searchModel);
        #endregion

        #region  PoOrderItem PDF
        /// <summary>
        /// Print po orders to pdf async
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="poOrders">PoOrder</param>
        /// <param name="language">Language</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the print po orders to pdf
        /// </returns>
        Task PrintPoOrdersToPdfAsync(Stream stream, IList<PoOrder> poOrders, Language language = null);

        /// <summary>
        /// Print po order To pdf async
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="poOrder">PoOrder</param>
        /// <param name="language">Language</param>
        /// <param name="store">Store</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the print po order to pdf
        /// </returns>
        Task PrintPoOrderToPdfAsync(Stream stream, PoOrder poOrder, Language language = null, Store store = null);

        /// <summary>
        /// Export po order item all to xlsx async
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order item all to xlsx
        /// </returns>
        Task<byte[]> ExportPoOrderItemAllToXlsxAsync();

        /// <summary>
        /// Export po order item to xlsx async
        /// </summary>
        /// <param name="selectid">selectid</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order item to xlsx
        /// </returns>
        Task<byte[]> ExportPoOrderItemToXlsxAsync(IList<int> selectid);

        /// <summary>
        /// Export po order to xlsx
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order to xlsx
        /// </returns>
        Task<byte[]> ExportPoOrderToXlsx(ManageInventorySearchModel searchModel);
        #endregion
    }
}
