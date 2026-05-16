using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Chat.Models;

/// <summary>
/// Represents configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Properties

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Chat.Fields.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Chat.Fields.Script")]
    public string Script { get; set; }
    public bool Script_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Chat.Fields.HeadMetaTag")]
    public string HeadMetaTag { get; set; }

    #endregion
}