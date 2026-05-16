namespace Nop.Plugin.Payments.Evance
{
    /// <summary>
    /// Represents the evance plugin defaults
    /// </summary>
    public class EvanceDefaults
    {
        /// <summary>
        /// Get or set the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Evance";

        /// <summary>
        /// Get or set the transaction settle parameters
        /// </summary>
        public static string IsTransactionSettleAttributeKey => "IsTransactionSettle";

        /// <summary>
        /// Gets the route name used for the customer vault
        /// </summary>
        public static string CustomerVaultRoute => "CustomerVault";

        /// <summary>
        /// Gets the route name used for the check status
        /// </summary>
        public static string CheckStatusRoute => "CheckStatus";

        /// <summary>
        /// Gets the route name used for the apple pay 
        /// </summary>
        public static string ApplePayRoute => "Plugin.Payments.Evance.ApplePay";
    }
}
