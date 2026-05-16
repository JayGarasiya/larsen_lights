using Nop.Core;

namespace Nop.Plugin.Payments.Evance.Domain
{
    /// <summary>
    /// Represents a customer vault
    /// </summary>
    public class CustomerVault : BaseEntity
    {
        /// <summary>
        /// Get or set a customer indentifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Get or set a customer vault indentifier
        /// </summary>
        public string CustomerVaultId { get; set; }
    }
}
