using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents an address validation model
/// </summary>
public record AddressValidationModel : BaseNopModel
{
    #region Ctor
    public AddressValidationModel()
    {
        AvailableAddress = new AddressModel();
    }
    #endregion

    #region Properties
    public bool HasAddress { get; set; }

    public string Message { get; set; }

    public bool IsError { get; set; }

    public bool ShowModel { get; set; }

    public bool IsNewAddress { get; set; }

    public int AddressId { get; set; }

    public AddressModel AvailableAddress { get; set; }
    #endregion
}