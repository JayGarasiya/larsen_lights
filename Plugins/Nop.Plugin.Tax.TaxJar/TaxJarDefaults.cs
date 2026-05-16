using Nop.Core.Caching;

namespace Nop.Plugin.Tax.TaxJar;

/// <summary>
/// Represents plugin constants
/// </summary>
public class TaxJarDefaults
{
    /// <summary>
    /// Gets the Avalara tax provider system name
    /// </summary>
    public static string SystemName => "Tax.TaxJar";

    /// <summary>
    /// Gets the key for caching tax rate
    /// </summary>
    /// <remarks>
    /// {0} - Customer id
    /// {1} - Address
    /// {2} - City
    /// {3} - State or province identifier
    /// {4} - Country identifier
    /// {5} - Zip postal code
    /// </remarks>
    public static CacheKey TaxRateCacheKey => new CacheKey("Nop.taxjar.taxrate.{0}-{1}-{2}-{3}-{4}-{5}");

    /// <summary>
    /// Gets the path of the content 
    /// </summary>
    public const string EXEMPTION_CERTIFICATE = "~/Plugins/Tax.TaxJar/Content/";

    /// <summary>
    /// Gets the name of the file
    /// </summary>
    public const string EXEMPTION_CERTIFICATE_FILE_NAME = "exemption-certificate.pdf";

    /// <summary>
    /// Gets the address lookup system name
    /// </summary>
    public static string AddressLookupSystemName => "Address.Lookup";

    /// <summary>
    /// Gets the generic attribute name to hide general settings block on the plugin configuration page
    /// </summary>
    public static string HideGeneralBlock => "TaxJarPage.HideGeneralBlock";

    /// <summary>
    /// Gets the generic attribute name to hide log block on the plugin configuration page
    /// </summary>
    public static string HideLogBlock => "TaxJarPage.HideLogBlock";
}