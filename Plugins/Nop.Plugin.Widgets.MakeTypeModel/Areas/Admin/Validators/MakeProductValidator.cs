using FluentValidation;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Validators
{
    /// <summary>
    /// Represents make product validator 
    /// </summary>
    public partial class MakeProductValidator : BaseNopValidator<MakeProductModel>
    {
        public MakeProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.Required"));

            SetDatabaseValidationRules<MakeProduct>("MakeProduct");
        }
    }
}