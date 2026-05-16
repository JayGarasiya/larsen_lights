namespace Nop.Plugin.Reports.ChartsAndTables.Domain
{
    /// <summary>
    /// Represents a chart and table data report line
    /// </summary>
    [Serializable]
    public partial class ChartsAndTablesData
    {
        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the total quantity
        /// </summary>
        public int TotalQuantity { get; set; }
    }
}
