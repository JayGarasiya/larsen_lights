using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents a Taxjar Request Log Search Model
/// </summary>
public record TaxJarRequestLogSearchModel : BaseSearchModel
{
    #region Properties
    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.Search.CreatedFrom")]
    [UIHint("DateNullable")]
    public DateTime? CreatedFrom { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Log.Search.CreatedTo")]
    [UIHint("DateNullable")]
    public DateTime? CreatedTo { get; set; }
    #endregion
}