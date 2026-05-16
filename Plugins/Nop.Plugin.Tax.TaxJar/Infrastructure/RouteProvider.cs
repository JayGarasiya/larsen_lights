using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Tax.TaxJar.Infrastructure;

/// <summary>
/// Represents provider that provided basic routes
/// </summary>
public class RouteProvider : IRouteProvider
{
    #region Methods
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Refund selected order items
        endpointRouteBuilder.MapControllerRoute("Plugin.Tax.TaxJar.RefundOrderSelected",
                                                "Admin/Order/RefundSelected",
                                                new { controller = "TaxJar", action = "RefundOrderSelected", area = AreaNames.ADMIN });
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 2;
    #endregion
}