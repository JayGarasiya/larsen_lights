using Nop.Web.Framework.Models;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents a Taxjar Request Log List Model
/// </summary>
public record TaxJarRequestLogListModel : BasePagedListModel<TaxJarRequestLogModel>
{
}