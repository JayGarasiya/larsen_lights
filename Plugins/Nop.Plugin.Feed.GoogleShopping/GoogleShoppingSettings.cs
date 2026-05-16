using Nop.Core.Configuration;

namespace Nop.Plugin.Feed.GoogleShopping;

/// <summary>
/// Represents Google Shopping plugin settings
/// </summary>
public class GoogleShoppingSettings : ISettings
{
    /// <summary>
    /// Product picture size
    /// </summary>
    public int ProductPictureSize { get; set; }

    /// <summary>
    /// A value indicating whether we should pass shipping info (weight)
    /// </summary>
    public bool PassShippingInfoWeight { get; set; }

    /// <summary>
    /// A value indicating whether we should pass shipping info (dimensions)
    /// </summary>
    public bool PassShippingInfoDimensions { get; set; }

    /// <summary>
    /// A value indicating whether we should calculate prices considering promotions (tier prices, discounts, special prices, etc)
    /// </summary>
    public bool PricesConsiderPromotions { get; set; }

    /// <summary>
    /// Currency identifier for which feed file(s) will be generated
    /// </summary>
    public int CurrencyId { get; set; }

    /// <summary>
    /// Default Google category
    /// </summary>
    public string DefaultGoogleCategory { get; set; }

    /// <summary>
    /// Static file name of the feed
    /// </summary>
    public string StaticFileName { get; set; }

    /// <summary>
    /// Number of days for expiration date
    /// </summary>
    public int ExpirationNumberOfDays { get; set; }
    
    /// <summary>
    /// File Counts for Google Feed file generate
    /// </summary>
    public int FileCounter { get; set; }
    
    /// <summary>
    /// get or sets page size for google XML products 
    /// </summary>
    public int PageSizeForGoogleXmlProducts { get; set; }
}