using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles.Models;

/// <summary>
/// Represents the model for configuring the discount rule based on customer roles.
/// </summary>
public class RequirementModel
{
    public RequirementModel()
    {
        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRole")]
    public IList<int> SelectedCustomerRoleIds { get; set; }

    public int DiscountId { get; set; }

    public int RequirementId { get; set; }

    public IList<SelectListItem> AvailableCustomerRoles { get; set; }
}