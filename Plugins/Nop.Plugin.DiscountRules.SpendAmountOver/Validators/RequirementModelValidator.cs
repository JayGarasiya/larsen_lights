using FluentValidation;
using Nop.Plugin.DiscountRules.SpendAmountOver.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.SpendAmountOver.Validators;

/// <summary>
/// Validator for the <see cref="RequirementModel"/> class.
/// </summary>
public class RequirementModelValidator : BaseNopValidator<RequirementModel>
{
    #region Ctor

    public RequirementModelValidator(ILocalizationService localizationService)
    {
        // Validate that DiscountId is not empty
        RuleFor(model => model.DiscountId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.SpendAmountOver.Fields.DiscountId.Required"));
        // Validate that SpendAmount is greater than 0
        RuleFor(model => model.SpendAmount)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.SpendAmountOver.Fields.SpendAmount.Required"));
    }

    #endregion
}
