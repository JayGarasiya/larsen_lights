using FluentValidation;
using Nop.Data;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.Fulfillment.Validators
{
    /// <summary>
    /// Represents an <see cref="ThreePlShippingMethodModel"/> validator.
    /// </summary>
    public partial class ThreePlShippingMethodValidator : BaseNopValidator<ThreePlShippingMethodModel>
    {
        public ThreePlShippingMethodValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            //if validation without this set rule is applied, in this case nothing will be validated
            //it's used to prevent auto-validation of child models
            RuleSet(NopValidationDefaults.ValidationRuleSet, () =>
            {
                RuleFor(model => model.ShippingMethod)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.ShippingMethod.Required"));

                RuleFor(model => model.ThreePlCarrier)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.ThreePlCarrier.Required"));

                RuleFor(model => model.ThreePlService)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.ThreePlService.Required"));

                SetDatabaseValidationRules<ThreePlShippingMethod>();
            });
        }
    }
}
