using FluentValidation;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.PriceImport;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Validators
{
    /// <summary>
    /// Represents price import validator 
    /// </summary>
    public partial class PriceImportValidator : BaseNopValidator<PriceImportModel>
    {
        public PriceImportValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.VendorId)
                .GreaterThanOrEqualTo(1)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.PriceImport.Vendor.Required"));

            SetDatabaseValidationRules<PriceImport>("PriceImport");
        }
    }
}
