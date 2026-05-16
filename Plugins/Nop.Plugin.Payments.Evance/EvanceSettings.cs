using Nop.Core.Configuration;
using Nop.Plugin.Payments.Evance.Domain;

namespace Nop.Plugin.Payments.Evance
{
    /// <summary>
    /// Represents evance settings
    /// </summary>
    public class EvanceSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether plugin enabled
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 3ds enabled
        /// </summary>
        public bool ThreedSEnable { get; set; }

        /// <summary>
        /// Gets or sets customer roles to exclude three ds
        /// </summary>
        public List<int> ExcludeThreeDSToCustomerRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customer vault enabled
        /// </summary>
        public bool CustomerVaultEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow save card
        /// </summary>
        public bool AllowSaveCard { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow save ACH
        /// </summary>
        public bool AllowSaveACH { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display saved details
        /// </summary>
        public bool DisplaySavedDetails { get; set; }

        /// <summary>
        /// Gets or sets an authentication key
        /// </summary>
        public string SecurityKey { get; set; }

        /// <summary>
        /// Gets or sets payment API base url
        /// </summary>
        public string PaymentAPIBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets an query API base url
        /// </summary>
        public string QueryAPIBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets a collect checkout key
        /// </summary>
        public string CollectCheckoutKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a payment options
        /// </summary>
        public List<int> PaymentOptions { get; set; }

        /// <summary>
        /// Gets or sets the transaction type
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to drop table
        /// </summary>
        public bool DropTable { get; set; }
    }
}
