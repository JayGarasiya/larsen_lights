using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Data;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Gets Mail Template Customer Role Service
    /// </summary>
    public partial class MailTemplateCustomerRoleService : IMailTemplateCustomerRoleService
    {
        #region Fields
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<MessageTemplate> _messageTemplaterepository;
        #endregion

        #region Ctor
        public MailTemplateCustomerRoleService(IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<MessageTemplate> messageTemplaterepository)
        {
            _genericAttributeRepository = genericAttributeRepository;
            _messageTemplaterepository = messageTemplaterepository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get list of message template ids by and role
        /// </summary>
        /// <param name="roleId">Role identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of generic attributes
        /// </returns>
        public virtual async Task<List<int>> GetMessageTemplateIdsByCustomerRoleIdAsync(int roleId = 0)
        {
            var query = _genericAttributeRepository.Table.Where(x => x.Key == InvoicePDFDefaults.FOR_CUSTOMER_ROLE);
            if (roleId > 0)
                query = query.Where(x => x.Value == roleId.ToString());

            return await query.Select(x => x.EntityId).ToListAsync();
        }

        /// <summary>
        /// Get list of message template ids by and roles
        /// </summary>
        /// <param name="roleIds">Role identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of generic attributes
        /// </returns>
        public virtual async Task<List<int>> GetMessageTemplateIdsByCustomerRoleIdsAsync(int[] roleIds)
        {
            if (roleIds == null)
                throw new ArgumentNullException(nameof(roleIds));

            var query = _genericAttributeRepository.Table.Where(x => x.Key == InvoicePDFDefaults.FOR_CUSTOMER_ROLE);
            if (roleIds.Any())
                query = query.Where(x => roleIds.Contains(int.Parse(x.Value)));

            return await query.Select(x => x.EntityId).ToListAsync();
        }

        /// <summary>
        /// Updates a message templates
        /// </summary>
        /// <param name="messageTemplateIds">Message template identifiers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateMessageTemplatesAsync(List<int> messageTemplateIds)
        {
            ArgumentNullException.ThrowIfNull(messageTemplateIds);

            var messageTemplateByIds = await _messageTemplaterepository.GetByIdsAsync(messageTemplateIds);
            if (!messageTemplateByIds.Any())
                return;

            messageTemplateByIds.ToList().ForEach(c => { c.IsActive = false; });
            await _messageTemplaterepository.UpdateAsync(messageTemplateByIds);
        }

        /// <summary>
        /// Gets list of message templates by identifiers
        /// </summary>
        /// <param name="messageTemplateIds">Message template identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of message template
        /// </returns>
        public virtual async Task<MessageTemplate> GetActiveMessageTemplatesByIdsAsync(List<int> messageTemplateIds)
        {
            ArgumentNullException.ThrowIfNull(messageTemplateIds);

            var query = from ga in _messageTemplaterepository.Table
                        where messageTemplateIds.Contains(ga.Id) && ga.IsActive
                        select ga;

            return await query.FirstOrDefaultAsync();
        }
        #endregion
    }
}
