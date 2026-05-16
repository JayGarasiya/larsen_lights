using Nop.Plugin.Widgets.MakeTypeModel.Domain;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services
{
    /// <summary>
    /// Custom return request service interface
    /// </summary>
    public interface ICustomReturnRequestService
    {
        /// <summary>
        /// Get Return Request Note By Indentifier
        /// </summary>
        /// <param name="requestId">request Indentifier</param>
        /// <returns>Get Return Request Note By Indentifier</returns>
        Task<RetuenRequestNote> GetReturnRequestNoteByIdAsync(int requestId);

        /// <summary>
        /// Insert Return Request Note
        /// </summary>
        /// <param name="returnRequestNote">Return Request Note</param>
        /// <returns>Insert Return Request Note</returns>
        Task InsertReturnRequestNoteAsync(RetuenRequestNote returnRequestNote);

        /// <summary>
        /// Update Return Request Note
        /// </summary>
        /// <param name="returnRequestNote">Return Request Note</param>
        /// <returns>Update Return Request Note</returns>
        Task UpdateReturnRequestNoteAsync(RetuenRequestNote returnRequestNote);
    }
}
