using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Evance.Domain;

namespace Nop.Plugin.Payments.Evance.Service
{
    /// <summary>
    /// Customer vault service interface
    /// </summary>
    public interface ICustomerVaultService
    {
        /// <summary>
        /// Inserts a customer vault
        /// </summary>
        /// <param name="customerVault">Customer vault</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertCustomerVaultAsync(CustomerVault customerVault);

        /// <summary>
        /// Get customer vaults by customer iderntifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<IList<CustomerVault>> GetCustomerVaultsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Delete a customer vault
        /// </summary>
        /// <param name="customerVault">Customer vault</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteCustomerVaultAsync(CustomerVault customerVault);

        /// <summary>
        /// Get customer vaults by iderntifier
        /// </summary>
        /// <param name="customerVaultId">Customer vault identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<CustomerVault> GetCustomerVaultByIdAsync(string customerVaultId);

        /// <summary>
        /// Get an order by transaction identifier
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <returns>Order</returns>
        Task<Order?> GetOrderByTransactionIdAsync(string transactionId);
    }
}