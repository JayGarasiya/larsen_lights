using FluentValidation;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Tax.TaxJar.Validators;

/// <summary>
/// Represents configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    #region Ctor
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.ApiToken)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.ApiToken.Required"));
    }
    #endregion
}