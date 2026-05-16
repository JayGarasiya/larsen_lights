using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.SpendAmountOver.Models;

/// <summary>
/// Represents the model for configuring the discount rule based on customer roles.
/// </summary>
public class RequirementModel
{
    public RequirementModel()
    {
        SelectedVendorIds = new List<int>();
        AvailableVendors = new List<SelectListItem>();
    }

    public int DiscountId { get; set; }

    public int RequirementId { get; set; }

    [NopResourceDisplayName("Plugins.DiscountRules.SpendAmountOver.Fields.Amount")]
    public decimal SpendAmount { get; set; }

    [NopResourceDisplayName("Plugins.DiscountRules.SpendAmountOver.Fields.Vendors")]
    public IList<int> SelectedVendorIds { get; set; }

    public IList<SelectListItem> AvailableVendors { get; set; }
}
