namespace Nop.Plugin.Payments.Evance.Domain
{
    /// <summary>
    /// Represents a transaction type
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Authorize Only
        /// </summary>
        AuthorizeOnly = 0,

        /// <summary>
        /// Sale (Authorization and Capture)
        /// </summary>
        AuthorizeAndCapture = 5
    }
}
