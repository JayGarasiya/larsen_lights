namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Inventory
{
    /// <summary>
    /// Represents saved summary dto
    /// </summary>
    public class StockSummaryDto
    {
        /// <summary>
        /// Gets or sets the Summaries
        /// </summary>
        public SummaryDto[] Summaries { get; set; }

        /// <summary>
        /// Gets or sets the _links
        /// </summary>
        public LinksDto _links { get; set; }

        #region Nested Classes

        /// <summary>
        /// Represents summary dto
        /// </summary>
        public class SummaryDto
        {
            /// <summary>
            /// Gets or sets the ItemIdentifier
            /// </summary>
            public ItemDto ItemIdentifier { get; set; }

            /// <summary>
            /// Gets or sets the Available
            /// </summary>
            public decimal Available { get; set; }

            #region Nested Classes

            /// <summary>
            /// Represents item dto
            /// </summary>
            public class ItemDto
            {
                /// <summary>
                /// Gets or sets the Sku
                /// </summary>
                public string Sku { get; set; }
            }

            #endregion
        }

        /// <summary>
        /// Represents links dto
        /// </summary>
        public class LinksDto
        {
            /// <summary>
            /// Gets or sets the Next
            /// </summary>
            public LinkDto Next { get; set; }

            #region Nested Classes

            /// <summary>
            /// Represents link dto
            /// </summary>
            public class LinkDto
            {
                /// <summary>
                /// Gets or sets the Href
                /// </summary>
                public string Href { get; set; }
            }

            #endregion
        }

        #endregion
    }
}
