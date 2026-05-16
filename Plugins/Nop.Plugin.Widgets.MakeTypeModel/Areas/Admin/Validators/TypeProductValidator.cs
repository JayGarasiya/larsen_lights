using FluentValidation;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Validators
{
    /// <summary>
    /// Represents type product validator 
    /// </summary>
    public partial class TypeProductValidator : BaseNopValidator<TypeProductModel>
    {
        public TypeProductValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.Required"));

            SetDatabaseValidationRules<TypeProduct>("TypeProduct");
        }
    }
}