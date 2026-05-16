using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Nop.Web.Framework;
using Nop.Web.Framework.Themes;

namespace Nop.Plugin.Widgets.ProductExtension.Infrastructure
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances to determine search paths for a view.
    /// </summary>
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

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
                viewLocations = new[] { $"/Plugins/Widgets.ProductExtension/Areas/Admin/Views/{context.ControllerName}/{context.ViewName}.cshtml" }.Concat(viewLocations);

            var controllerName = context.ControllerName;
            var viewName = context.ViewName;

            if (context.ControllerName.Equals("ProductReview") || context.ViewName.Contains("_Availability"))
                controllerName = "Product";

            if (context.ViewName.Contains("ProductDetailReviews"))
                viewName = "ProductDetailReviews";

            if (context.Values.TryGetValue(THEME_KEY, out string theme))
                viewLocations = new[] {
                    $"/Plugins/Widgets.ProductExtension/Themes/{theme}/Views/{controllerName}/{viewName}.cshtml",
                    $"/Plugins/Widgets.ProductExtension/Themes/{theme}/Views/Shared/{viewName}.cshtml",
                    $"/Plugins/Widgets.ProductExtension/Views/{controllerName}/{viewName}.cshtml"
                }.Concat(viewLocations);

            return viewLocations;
        }
    }
}
