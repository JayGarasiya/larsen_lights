using Nop.Core.Configuration;
using Nop.Plugin.Tax.TaxJar.Domain;

namespace Nop.Plugin.Tax.TaxJar;

/// <summary>
/// Represents the settings for the TaxJar plugin.
/// </summary>
public class TaxJarSettings : ISettings
{
    public TaxJarSettings()
    {
        IsTaxExempt = new List<int>();
        ExcludeTaxOnStateProvinceIds = new List<int>();
    }

    /// <summary>
    /// Gets or sets TaxJar API Token
    /// </summary>
    public string ApiToken { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use sandbox (testing environment)
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets IsTaxExempt customer attributes 
    /// </summary>
    public List<int> IsTaxExempt { get; set; }

    /// <summary>
    /// Gets or sets restict payment methods
    /// </summary>
    public string PaymentMethods { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to commit tax transactions right after they are saved
    /// </summary>
    public bool CommitTransactions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate entered by customer addresses before the tax calculation
    /// </summary>
    public bool ValidateAddress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate customer billing addresse
    /// </summary>
    public bool ValidateBilling { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate customer shipping addresse
    /// </summary>
    public bool ValidateShipping { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate customer only new billing / shipping addresse
    /// </summary>
    public bool OnlyNewAddress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate customer for country
    /// </summary>
    public int CountryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to CC attribute
    /// </summary>
    public int CCAttributeId { get; set; }

    /// <summary>
    /// Gets or sets a type of the tax origin address
    /// </summary>
    public TaxOriginAddressType TaxOriginAddressType { get; set; }

    /// <summary>
    /// Gets or sets exclude tax on states / provinces
    /// </summary>
    public List<int> ExcludeTaxOnStateProvinceIds { get; set; }

    /// <summary>
    /// Gets or sets the tax rate (by address) cache time in minutes
    /// </summary>
    public int TaxRateByAddressCacheTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating we should attach PDF invoice to "Order refund" email
    /// </summary>
    public bool AttachPdfInvoiceToOrderRefundSelectedEmail { get; set; }

    /// <summary>
    /// Gets or sets the hidden customer role to allow impersonated entry for product
    /// </summary>
    public int HiddenCustomerRoleId { get; set; }

    /// <summary>
    /// Gets or sets the TaxExclusionOnCall
    /// </summary>
    public bool TaxExclusionOnCall { get; set; }

    /// <summary>
    /// Gets or sets the TaxExclusionOnCall
    /// </summary>
    public string TaxExclusionOn { get; set; }

    public List<TaxExclusionOn> TaxExclusions()
    {
        if (!string.IsNullOrEmpty(TaxExclusionOn))
        {
            return TaxExclusionOn.Split(';').Select(t => 
            new TaxExclusionOn()
            {
                ControllerName = t.Split(',')[0]?.ToString().Trim(),
                ActionName = t.Split(',')[1]?.ToString().Trim(),
                Method = t.Split(',')[2]?.ToString().Trim(),
            }).ToList();
        }
        return new List<TaxExclusionOn>();
    }
}
