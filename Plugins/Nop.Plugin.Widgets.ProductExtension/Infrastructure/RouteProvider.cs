using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.ProductExtension.Infrastructure
{
    /// <summary>
    /// Represents provider that provided basic routes
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

            //download quote
            endpointRouteBuilder.MapControllerRoute(name: "DownloadQuote",
                pattern: $"download/quote/{{customerGuid?}}",
                defaults: new { controller = "ProductReview", action = "CartQuote" });

            //change price
            endpointRouteBuilder.MapControllerRoute(name: "ChangePrice",
                pattern: $"{lang}/changeprice/{{customerprice:min(0)}}",
                defaults: new { controller = "ProductReview", action = "SetPrice" });
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