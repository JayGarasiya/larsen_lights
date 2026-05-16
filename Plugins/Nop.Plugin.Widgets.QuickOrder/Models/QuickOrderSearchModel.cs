using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.QuickOrder.Models;

/// <summary>
/// Represents an quick order search model
/// </summary>
public partial record QuickOrderSearchModel : CustomerSearchModel
{
    #region Properties

    [NopResourceDisplayName("Plugins.Widgets.QuickOrder.List.CustomOrderNumber")]
    public string CustomOrderNumber { get; set; }

    public new bool AvatarEnabled { get; internal set; }

    #endregion
}
