using Nop.Web.Framework.Models;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents an address lookup model
/// </summary>
public record AddressLookupModel : BaseNopModel
{
    #region Properties
    public string ButtonClass { get; set; }

    public string Prefix { get; set; }

    public string FormId { get; set; }

    public string SelectListId { get; set; }

    public bool OnePageCheckoutEnabled { get; set; }

    public bool IsShippingAddress { get; set; }

    public bool Validate { get; set; }

    public int DefaultAddressId { get; set; }

    public bool UseDefaultAddress { get; set; }

    #endregion
}