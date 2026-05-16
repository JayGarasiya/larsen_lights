using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Nop.Plugin.Widgets.Chat.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.Chat.Components;

/// <summary>
/// Represents the view component to place a widget into pages
/// </summary>
public class ChatScriptViewComponent : NopViewComponent
{
    #region Fields
    private readonly ChatScriptService _chatScriptService;
    private readonly ChatScriptSettings _chatScriptSettings;
    #endregion

    #region Ctor
    public ChatScriptViewComponent(
        ChatScriptService chatScriptService,
        ChatScriptSettings chatScriptSettings)
    {
        _chatScriptService = chatScriptService;
        _chatScriptSettings = chatScriptSettings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="widgetZone">Widget zone name</param>
    /// <param name="additionalData">Additional data for the widget</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Check if the widget should be placed in the 'HeadHtmlTag' zone
        if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
            // Return the meta tag from settings for the head section
            return new HtmlContentViewComponentResult(new HtmlString(_chatScriptSettings.HeadMetaTag ?? string.Empty));

        // If the widget is in the 'BodyEndHtmlTagBefore' zone, prepare the script
        var script = widgetZone != PublicWidgetZones.BodyEndHtmlTagBefore
            ? string.Empty  // If not, return an empty script
            : await _chatScriptService.PrepareScriptAsync(); // Otherwise, get the script from the service

        // Return the script (empty or the actual script content) wrapped in a HtmlString
        return new HtmlContentViewComponentResult(new HtmlString(script ?? string.Empty));
    }
    #endregion
}