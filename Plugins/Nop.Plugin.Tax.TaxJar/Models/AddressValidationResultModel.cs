using Nop.Web.Framework.Models;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents an address validation result model
/// </summary>
public record AddressValidationResultModel : BaseNopModel
{
    #region Ctor
    public AddressValidationResultModel()
    {
        Warnings = new List<string>();
    }
    #endregion

    #region Properties
    public string CurrentAddress { get; set; }

    public string ValidAddress { get; set; }

    public bool IsError { get; set; }

    public IList<string> Warnings { get; set; }
    #endregion
}