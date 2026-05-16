using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.ShipmentTracking.Domain;

namespace Nop.Plugin.Misc.ShipmentTracking.Services
{
    /// <summary>
    /// Represents an admin note service
    /// </summary>
    public partial class AdminNoteService : IAdminNoteService
    {
        #region Fields

        protected readonly IRepository<AdminNote> _adminNoteRepository;
        protected readonly IRepository<OrderNote> _orderNoteRepository;

        #endregion

        #region Ctor

        public AdminNoteService(IRepository<AdminNote> adminNoteRepository,
            IRepository<OrderNote> orderNoteRepository)
        {
            _adminNoteRepository = adminNoteRepository;
            _orderNoteRepository = orderNoteRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a list admin notes of order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="note">Value indicating whether a search in a note; pass null to ignore</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<IPagedList<OrderNote>> GetAdminNotesByOrderIdAsync(int orderId, string note = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if(orderId == 0)
                return new PagedList<OrderNote>(new List<OrderNote>(), pageIndex, pageSize);

            var query = from orderNote in _orderNoteRepository.Table
                        join adminNote in _adminNoteRepository.Table on orderNote.Id equals adminNote.OrderNoteId
                        where orderId == adminNote.OrderId
                        select orderNote; 

            //filter by note
            if (!string.IsNullOrEmpty(note))
                query = query.Where(o => o.Note.Contains(note));

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets an order latest admin note
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order note
        /// </returns>
        public virtual async Task<OrderNote> GetLatestAdminNoteByOrderIdAsync(int orderId)
        {
            if (orderId == 0)
                return null;

            return await (from orderNote in _orderNoteRepository.Table
                          join adminNote in _adminNoteRepository.Table on orderNote.Id equals adminNote.OrderNoteId
                          where adminNote.OrderId == orderId
                          orderby orderNote.CreatedOnUtc descending
                          select orderNote).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts an order admin note
        /// </summary>
        /// <param name="adminNote">The order admin note</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order note
        /// </returns>
        public virtual async Task InsertAdminNoteAsync(AdminNote adminNote)
        {
            await _adminNoteRepository.InsertAsync(adminNote);
        }

        #endregion
    }
}
