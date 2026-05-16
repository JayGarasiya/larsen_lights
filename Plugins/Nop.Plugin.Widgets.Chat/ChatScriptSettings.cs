using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.Chat;

/// <summary>
/// Represents plugin settings
/// </summary>
public class ChatScriptSettings : ISettings
{
    /// <summary>
    /// Gets or sets an installation script
    /// </summary>
    public string Script { get; set; }

    /// <summary>
    /// Gets or sets a head meta tag
    /// </summary>
    public string HeadMetaTag { get; set; }
}
