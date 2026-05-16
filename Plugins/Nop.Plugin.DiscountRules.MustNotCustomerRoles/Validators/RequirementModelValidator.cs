using FluentValidation;
using Nop.Plugin.DiscountRules.MustNotCustomerRoles.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles.Validators;

/// <summary>
/// Represents a validator for the <see cref="RequirementModel"/>.
/// </summary>
public class RequirementModelValidator : BaseNopValidator<RequirementModel>
{
    public RequirementModelValidator(ILocalizationService localizationService)
    {
        // Validate that DiscountId is not empty (required field)
        RuleFor(model => model.DiscountId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.MustNotCustomerRoles.Fields.DiscountId.Required"));

        // Validate that at least one customer role is selected
        RuleFor(x => x.SelectedCustomerRoleIds).Must((x, context) =>
        {
            // Check if any customer role IDs are selected
            var hasRoles = x.SelectedCustomerRoleIds.Any();
            if (hasRoles)
                return true;  // Validation passes if roles are selected

            return false;  // Validation fails if no roles are selected
        }).WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRoleId.Required"));  // Custom error message if no roles are selected
    }
}
