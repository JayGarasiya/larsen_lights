using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Infrastructure;

/// <summary>
/// Represents the route provider for the "Previously Purchased" plugin.
/// This class registers custom routes for the plugin's functionality.
/// </summary>
public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    #region Methods
    /// <summary>
    /// Registers the custom routes for the PreviouslyPurchased plugin.
    /// </summary>
    /// <param name="endpointRouteBuilder">The route builder used to register routes.</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {

        // Get the language route pattern for localized URLs.
        // We don't need to apply this pattern for AJAX requests or actions returning direct results (like file downloads).
        // It's only used for URLs that a user can navigate to in a browser.
        var lang = GetLanguageRoutePattern();

        // Register a route for accessing the "Previously Purchased Products" page
        endpointRouteBuilder.MapControllerRoute(name: PreviouslyPurchasedDefaults.PreviouslyPurchasedProductsRoute,
            pattern: $"{lang}/previouslypurchasedproducts/",
            defaults: new { controller = "PreviouslyPurchasedCatalog", action = "PreviouslyPurchasedProducts" });

    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the priority of this route provider.
    /// </summary>
    public int Priority => 0;
    #endregion
}