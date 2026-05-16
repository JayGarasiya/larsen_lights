using Nop.Core.Domain.Messages;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Get list of IMail Template Customer Role Service
    /// </summary>
    public interface IMailTemplateCustomerRoleService
    {
        /// <summary>
        /// Get list of message template ids by and role
        /// </summary>
        /// <param name="roleId">Role identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of generic attributes
        /// </returns>
        Task<List<int>> GetMessageTemplateIdsByCustomerRoleIdAsync(int roleId = 0);

        /// <summary>
        /// Get list of message template ids by and roles
        /// </summary>
        /// <param name="roleIds">Role identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of generic attributes
        /// </returns>
        Task<List<int>> GetMessageTemplateIdsByCustomerRoleIdsAsync(int[] roleIds);

        /// <summary>
        /// Updates a message templates
        /// </summary>
        /// <param name="messageTemplateIds">Message template identifiers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateMessageTemplatesAsync(List<int> messageTemplateIds);

        /// <summary>
        /// Gets list of message templates by identifiers
        /// </summary>
        /// <param name="messageTemplateIds">Message template identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of message template
        /// </returns>
        Task<MessageTemplate> GetActiveMessageTemplatesByIdsAsync(List<int> messageTemplateIds);
    }
}
