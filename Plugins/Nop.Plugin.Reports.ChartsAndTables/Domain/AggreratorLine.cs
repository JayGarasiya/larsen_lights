namespace Nop.Plugin.Reports.ChartsAndTables.Domain
{
    /// <summary>
    /// Represents an chart and table aggrerator report line
    /// </summary>
    public partial class AggreratorLine
    {
        /// <summary>
        /// Gets or sets the total sales value
        /// </summary>
        public decimal TotalSalesValue { get; set; }

        /// <summary>
        /// Gets or sets the average sales value
        /// </summary>
        public decimal AverageSalesValue { get; set; }

        /// <summary>
        /// Gets or sets the minimume sales value
        /// </summary>
        public decimal MinSalesValue { get; set; }

        /// <summary>
        /// Gets or sets the maximume sales value
        /// </summary>
        public decimal MaxSalesValue { get; set; }

        /// <summary>
        /// Gets or sets the total quantity
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// Gets or sets the average quantity
        /// </summary>
        public decimal AverageQuantity { get; set; }

        /// <summary>
        /// Gets or sets the minimume quantity
        /// </summary>
        public int MinQuantity { get; set; }

        /// <summary>
        /// Gets or sets the maximume quantity
        /// </summary>
        public int MaxQuantity { get; set; }
    }
}
