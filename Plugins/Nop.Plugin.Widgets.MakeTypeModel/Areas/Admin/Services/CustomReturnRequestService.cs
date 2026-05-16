using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Domain;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services
{
    /// <summary>
    /// Custom return request service
    /// </summary>
    public class CustomReturnRequestService : ICustomReturnRequestService
    {
        #region Fields

        protected readonly IRepository<RetuenRequestNote> _returnRequestNoteRepository;

        #endregion

        #region Ctor

        public CustomReturnRequestService(IRepository<RetuenRequestNote> returnRequestNoteRepository)
        {
            _returnRequestNoteRepository = returnRequestNoteRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get Return Request Note By Indentifier
        /// </summary>
        /// <param name="requestId">request Indentifier</param>
        /// <returns>Get Return Request Note By Indentifier</returns>
        public virtual Task<RetuenRequestNote> GetReturnRequestNoteByIdAsync(int requestId)
        {
            var query = from rq in _returnRequestNoteRepository.Table
                        where rq.RetuenRequestId == requestId
                        select rq;

            return Task.FromResult(query.FirstOrDefault());
        }

        /// <summary>
        /// Insert Return Request Note
        /// </summary>
        /// <param name="returnRequestNote">Return Request Note</param>
        /// <returns>Insert Return Request Note</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertReturnRequestNoteAsync(RetuenRequestNote returnRequestNote)
        {
            ArgumentNullException.ThrowIfNull(returnRequestNote);

            await _returnRequestNoteRepository.InsertAsync(returnRequestNote);
        }

        /// <summary>
        /// Update Return Request Note
        /// </summary> 
        /// <param name="returnRequestNote">Return Request Note</param>
        /// <returns>Update Return Request Note</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateReturnRequestNoteAsync(RetuenRequestNote returnRequestNote)
        {
            ArgumentNullException.ThrowIfNull(returnRequestNote);

            await _returnRequestNoteRepository.UpdateAsync(returnRequestNote);
        }

        #endregion
    }
}
