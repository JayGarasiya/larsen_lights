using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Payments.Evance.Domain;

namespace Nop.Plugin.Payments.Evance.Service
{
    /// <summary>
    /// Customer vault service
    /// </summary>
    public class CustomerVaultService : ICustomerVaultService
    {
        #region Fields

        private readonly IRepository<CustomerVault> _customerVaultRepository;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        public CustomerVaultService(IRepository<CustomerVault> customerVaultRepository, IRepository<Order> orderRepository)
        {
            _customerVaultRepository = customerVaultRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a customer vault
        /// </summary>
        /// <param name="customerVault">Customer vault</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertCustomerVaultAsync(CustomerVault customerVault)
        {
            await _customerVaultRepository.InsertAsync(customerVault);
        }

        /// <summary>
        /// Get customer vaults by customer iderntifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<IList<CustomerVault>> GetCustomerVaultsByCustomerIdAsync(int customerId)
        {
            var query = await _customerVaultRepository.Table
                        .Where(x => x.CustomerId == customerId)
                        .OrderByDescending(x=>x.Id)
                        .ToListAsync();

            return query.ToList();
        }

        /// <summary>
        /// Get customer vaults by iderntifier
        /// </summary>
        /// <param name="customerVaultId">Customer vault identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<CustomerVault> GetCustomerVaultByIdAsync(string customerVaultId)
        {
            var query = _customerVaultRepository.Table
                        .Where(x => x.CustomerVaultId == customerVaultId)
                        .FirstOrDefaultAsync();
            return await query;
        }

        /// <summary>
        /// Delete a customer vault
        /// </summary>
        /// <param name="customerVault">Customer vault</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteCustomerVaultAsync(CustomerVault customerVault)
        {
            await _customerVaultRepository.DeleteAsync(customerVault);
        }

        #region Webhook

        /// <summary>
        /// Get an order by transaction identifier
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <returns>Order</returns>
        public virtual async Task<Order?> GetOrderByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return null;

            // First check: AuthorizationTransactionId
            var order = await _orderRepository.Table
                .Where(o => o.AuthorizationTransactionId == transactionId &&
                            o.PaymentMethodSystemName == EvanceDefaults.SystemName)
                .OrderByDescending(o => o.CreatedOnUtc)
                .FirstOrDefaultAsync();

            if (order != null)
                return order;

            // Second check: CaptureTransactionId
            order = await _orderRepository.Table
                .Where(o => o.CaptureTransactionId == transactionId &&
                            o.PaymentMethodSystemName == EvanceDefaults.SystemName)
                .OrderByDescending(o => o.CreatedOnUtc)
                .FirstOrDefaultAsync();

            return order;
        }

        #endregion

        #endregion

    }
}
