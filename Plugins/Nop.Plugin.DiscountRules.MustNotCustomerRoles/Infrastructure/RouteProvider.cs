using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles.Infrastructure;

/// <summary>
/// Represents the route provider for the "MustNotCustomerRoles" plugin.
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Registers the routes for the plugin.
    /// </summary>
    /// <param name="endpointRouteBuilder">The route builder that is used to define routes.</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Maps a custom route for the plugin's configuration page in the admin area
        endpointRouteBuilder.MapControllerRoute(
            name: DiscountRequirementDefaults.ConfigurationRouteName,
            pattern: "Admin/DiscountRulesMustNotCustomerRoles/Configure",
            defaults: new { controller = "DiscountRulesMustNotCustomerRoles", action = "Configure", area = AreaNames.ADMIN }
        );
    }

    /// <summary>
    /// Gets the priority of the route provider.
    /// </summary>
    public int Priority => 0;
}
