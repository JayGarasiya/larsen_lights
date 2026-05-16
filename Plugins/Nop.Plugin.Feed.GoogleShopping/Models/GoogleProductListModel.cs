using Nop.Web.Framework.Models;

namespace Nop.Plugin.Feed.GoogleShopping.Models;

/// <summary>
/// Represents Google Product List Model
/// </summary>
public record GoogleProductListModel : BasePagedListModel<GoogleProductModel>
{
}
