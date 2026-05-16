using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.BulkEdit.Infrastructure;

/// <summary>
/// Specifies the contracts for a view location expander 
/// that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances 
/// to determine search paths for a view.
/// </summary>
public class ViewLocationExpander : IViewLocationExpander
{
    /// <summary>
    /// Invoked by a <see cref="RazorViewEngine"/> to determine potential locations for a view.
    /// </summary>
    /// <param name="context">The <see cref="ViewLocationExpanderContext"/> for the current view location
    /// expansion operation.</param>
    /// <param name="viewLocations">The sequence of view locations to expand.</param>
    /// <returns>A list of expanded view locations.</returns>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName is null)
            return viewLocations;

        if (context.AreaName.Equals(AreaNames.ADMIN) && context.ControllerName.Equals("Product") && context.ViewName.Equals("BulkEdit"))
            return new[] { $"~/Plugins/Misc.BulkEdit/Areas/Admin/Views/Product/BulkEdit.cshtml" }.Concat(viewLocations);

        return viewLocations;
    }

    /// <summary>
    /// Invoked by a <see cref="RazorViewEngine"/> to determine the values that would be consumed by this instance
    /// of <see cref="IViewLocationExpander"/>. The calculated values are used to determine if the view location
    /// has changed since the last time it was located.
    /// </summary>
    /// <param name="context">The <see cref="ViewLocationExpanderContext"/> for the current view location
    /// expansion operation.</param>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }
}
