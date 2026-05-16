namespace Nop.Plugin.Payments.Evance.Domain
{
    /// <summary>
    /// Represents a payment options
    /// </summary>
    public enum PaymentOptions
    {
        /// <summary>
        /// Card payment
        /// </summary>
        Card_Payment = 10,

        /// <summary>
        /// Google pay
        /// </summary>
        Google_Pay = 20,

        /// <summary>
        /// Apple pay
        /// </summary>
        Apple_Pay = 30,

        /// <summary>
        /// ACH Payment
        /// </summary>
        ACH_Payment = 40,
    }
}
