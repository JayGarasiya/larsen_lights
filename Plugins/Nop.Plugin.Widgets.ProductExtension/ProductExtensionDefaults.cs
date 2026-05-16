namespace Nop.Plugin.Widgets.ProductExtension
{
    /// <summary>
    /// Represents plugin default vaues and constants
    /// </summary>
    public class ProductExtensionDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Widgets.ProductExtension";

        /// <summary>
        /// Gets the name of the view component widget zone into product detail pages
        /// </summary>
        public static string ProductDetailsAfterCollateral => "productdetails_after_collateral";

        /// <summary>
        /// Gets the name of the view component to place a widget into admin product edit pages
        /// </summary>
        public const string VIEW_COMPONENT_PRODUCTREVIEW = "ProductDetailReviews";

        /// <summary>
        /// Gets a name of generic attribute to store the value of 'DealerPrice'
        /// </summary>
        public static string DealerPriceAttribute => "DealerPrice";

        /// <summary>
        /// Gets the product notes site map system name
        /// </summary>
        public static string ProductNotesSystemName => "ProductExtension.ProductNotes";

        /// <summary>
        /// Gets the route name used to change the product price
        /// </summary>
        public static string ChangePriceRoute => "ChangePrice";

        /// <summary>
        /// Gets the route name used to download the quote
        /// </summary>
        public static string DownloadQuoteRoute => "DownloadQuote";

    }
} 