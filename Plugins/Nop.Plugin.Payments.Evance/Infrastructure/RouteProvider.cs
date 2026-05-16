using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Evance.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: EvanceDefaults.ApplePayRoute, 
                pattern: ".well-known/{folder}",
                defaults: new { controller = "Evance", action = "GetFile" });

            endpointRouteBuilder.MapControllerRoute(
                name: EvanceDefaults.CustomerVaultRoute,
                pattern: $"customer/customerVaultDetails",
                defaults: new { controller = "Evance", action = "CustomerVaultList" }
            );

            endpointRouteBuilder.MapControllerRoute(
                name: EvanceDefaults.CheckStatusRoute,
                pattern: "Evance/CheckStatus",
                defaults: new { controller = "Evance", action = "CheckStatus" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;

        #endregion
    }
}
