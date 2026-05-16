using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.MakeTypeModel.Components
{
    /// <summary>
    /// Represents the view component of product models details
    /// </summary>
    public class ProductModelsDetailViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ProductModelsDetailViewComponent(IMakeTypeModelService makeTypeModelService,
            ILocalizationService localizationService)
        {
            _makeTypeModelService = makeTypeModelService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone != AdminWidgetZones.ProductDetailsBlock)
                return Content(string.Empty);

            if (!(additionalData is ProductModel productModel))
                return Content(string.Empty);

            //check plugin was enable 
            if (!await _makeTypeModelService.PluginActiveAsync())
                return Content(string.Empty);

            var model = new ModelProductSearchModel() { ProductId = productModel.Id };

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelsList.cshtml", model);
        }

        #endregion
    }
}