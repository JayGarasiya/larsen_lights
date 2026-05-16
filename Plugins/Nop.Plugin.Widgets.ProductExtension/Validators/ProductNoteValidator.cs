using FluentValidation;
using Nop.Data;
using Nop.Plugin.Widgets.ProductExtension.Domain;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.ProductExtension.Validators
{
    public partial class ProductNoteValidator : BaseNopValidator<ProductNoteModel>
    {
        public ProductNoteValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Fields.Name.Required"));
            
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Fields.Description.Required"));
            
            RuleFor(x => x.WidgetZone)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Fields.WidgetZone.Required"));

            SetDatabaseValidationRules<ProductNote>();
        }
    }
}