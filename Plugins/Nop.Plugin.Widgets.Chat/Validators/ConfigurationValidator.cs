using FluentValidation;
using Nop.Plugin.Widgets.Chat.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.Chat.Validators;

/// <summary>
/// Represents configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    #region Ctor
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        // Validate that 'Script' field must not be empty when 'Enabled' field is true
        RuleFor(model => model.Script)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.Chat.Fields.Script.Required"))
            .When(model => model.Enabled);
    }
    #endregion
}