using FluentValidation;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Validators
{
    /// <summary>
    /// Represents model product validator 
    /// </summary>
    public partial class ModelProductValidator : BaseNopValidator<ModelProductModel>
    {
        public ModelProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Name.Required"));

            RuleFor(x => x.MakeName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.MakeName.Required"));

            RuleFor(x => x.TypeName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.TypeName.Required"));

            SetDatabaseValidationRules<ModelProduct>("ModelProduct");
        }
    }
}