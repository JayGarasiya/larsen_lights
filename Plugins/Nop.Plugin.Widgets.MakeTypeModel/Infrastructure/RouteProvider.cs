using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.MakeTypeModel.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //product search
            endpointRouteBuilder.MapControllerRoute(name: "ProductSearch",
                pattern: $"{lang}/search/",
                defaults: new { controller = "FindParts", action = "Search" });
            
            //Catalog products (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "SearchProducts",
                pattern: $"product/search",
                defaults: new { controller = "FindParts", action = "SearchProducts" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 2;

        #endregion
    }
}