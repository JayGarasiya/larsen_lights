using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework;
using Nop.Web.Framework.Themes;

namespace Nop.Plugin.OnePage.Checkout.Infrastructure
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances to determine search paths for a view.
    /// </summary>
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        /// <summary>
        /// Get the route name associated with the request rendering this page
        /// </summary>
        /// <returns>Route name</returns>
        private string GetRouteName(ViewLocationExpanderContext context)
        {
            //or try to get a registered endpoint route name
            var endpointFeature = context.ActionContext.HttpContext.Features.Get<IEndpointFeature>();
            var routeNameMetadata = endpointFeature.Endpoint.Metadata.GetMetadata<RouteNameMetadata>();
            return routeNameMetadata.RouteName;
        }

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
        /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
        /// The calculated values are used to determine if the view location has changed since the last time it was located.
        /// </summary>
        /// <param name="context">Context</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            //no need to add the themeable view locations at all as the administration should not be themeable anyway
            if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
                return;

            context.Values[THEME_KEY] = EngineContext.Current.Resolve<IThemeContext>().GetWorkingThemeNameAsync().Result;
        }

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="viewLocations">View locations</param>
        /// <returns>iew locations</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
                return viewLocations;

            var storeScope = EngineContext.Current.Resolve<IStoreContext>().GetActiveStoreScopeConfigurationAsync().Result;
            var onePageCheckoutSettings = EngineContext.Current.Resolve<ISettingService>().LoadSettingAsync<OnePageCheckoutSettings>(storeScope).Result;

            var routeName = GetRouteName(context);

            if(routeName?.Equals("CheckoutCompleted", StringComparison.InvariantCultureIgnoreCase) ?? false)
                return viewLocations;

            if ((context.ControllerName.Contains("Checkout", StringComparison.InvariantCultureIgnoreCase) ||
                context.ControllerName.Contains("Customer", StringComparison.InvariantCultureIgnoreCase) ||
                (routeName?.Equals("UpdateOrderSummary", StringComparison.InvariantCultureIgnoreCase) ?? false)) && 
                onePageCheckoutSettings.EnableOnePageCheckout)
            {
                context.Values.TryGetValue(THEME_KEY, out string theme);
                viewLocations = new[] {
                    $"/Plugins/OnePage.Checkout/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/OnePage.Checkout/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/OnePage.Checkout/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/OnePage.Checkout/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
