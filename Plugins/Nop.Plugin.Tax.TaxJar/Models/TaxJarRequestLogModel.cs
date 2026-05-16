using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents a Taxjar Request Log Model
/// </summary>
public record TaxJarRequestLogModel : BaseNopEntityModel
{
    #region Properties
    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.StatusCode")]
    public int StatusCode { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.Url")]
    public string Url { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.RequestMessage")]
    public string RequestMessage { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.ResponseMessage")]
    public string ResponseMessage { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.Customer")]
    public int? CustomerId { get; set; }
    public string CustomerEmail { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.CreatedDate")]
    public DateTime CreatedDate { get; set; }
    #endregion
}