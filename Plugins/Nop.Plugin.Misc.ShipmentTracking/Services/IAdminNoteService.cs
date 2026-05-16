using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.ShipmentTracking.Domain;

namespace Nop.Plugin.Misc.ShipmentTracking.Services
{
    /// <summary>
    /// Represents an admin note service interface
    /// </summary>
    public partial interface IAdminNoteService
    {
        #region AdminNote

        /// <summary>
        /// Gets a list admin notes of order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="note">Value indicating whether a search in a note; pass null to ignore</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<IPagedList<OrderNote>> GetAdminNotesByOrderIdAsync(int orderId, string note = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets an order latest admin note
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order note
        /// </returns>
        Task<OrderNote> GetLatestAdminNoteByOrderIdAsync(int orderId);

        /// <summary>
        /// Inserts an order admin note
        /// </summary>
        /// <param name="adminNote">The order admin note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAdminNoteAsync(AdminNote adminNote);

        #endregion
    }
}
