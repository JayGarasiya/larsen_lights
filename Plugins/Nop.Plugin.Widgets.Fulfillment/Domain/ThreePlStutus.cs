namespace Nop.Plugin.Widgets.Fulfillment.Domain
{
    /// <summary>
    /// Represents 3pl central sync status enumeration
    /// </summary>
    public enum ThreePlStutus
    {
        /// <summary>
        /// Pending (default)
        /// </summary>
        Pending = 10,

        /// <summary>
        /// Pending Stock
        /// </summary>
        PendingStock = 15,

        /// <summary>
        /// Sent
        /// </summary>
        Sent = 20,

        /// <summary>
        /// Partially Sent
        /// </summary>
        PartiallySent = 25,

        /// <summary>
        /// Error
        /// </summary>
        Error = 30,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel = 40,

        /// <summary>
        /// Cancel
        /// </summary>
        PartiallyCancel = 45,

        /// <summary>
        /// Cancel
        /// </summary>
        NotRequire = 50,

    }
}
