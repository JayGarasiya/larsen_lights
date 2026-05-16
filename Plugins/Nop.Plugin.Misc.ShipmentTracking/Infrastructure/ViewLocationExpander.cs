using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.ShipmentTracking.Infrastructure
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances to determine search paths for a view.
    /// </summary>
    public class ViewLocationExpander : IViewLocationExpander
    {
        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
        /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
        /// The calculated values are used to determine if the view location has changed since the last time it was located.
        /// </summary>
        /// <param name="context">Context</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <summary>
        /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="viewLocations">View locations</param>
        /// <returns>iew locations</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var controllerName = context.ControllerName.Equals("OrderShipment", System.StringComparison.InvariantCultureIgnoreCase) ? "Order" : context.ControllerName;
            if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
                viewLocations = new[] {
                    $"/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/{controllerName}/{context.ViewName}.cshtml",
                    $"/Areas/Admin/Views/{controllerName}/{context.ViewName}.cshtml"
                }.Concat(viewLocations);
            
            return viewLocations;
        }
    }
}
