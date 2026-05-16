using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.MakeTypeModel.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.MakeTypeModel.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class MakeTypeModelViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public MakeTypeModelViewComponent(IMakeTypeModelService makeTypeModelService,
            ILocalizationService localizationService,
            ICategoryService categoryService)
        {
            _makeTypeModelService = makeTypeModelService;
            _localizationService = localizationService;
            _categoryService = categoryService;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            if (widgetZone != PublicWidgetZones.HeaderMenuAfter)
                return Content(string.Empty);

            //check plugin was enable 
            if (!await _makeTypeModelService.PluginActiveAsync())
                return Content(string.Empty);

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            if(!makes.Any())
                return Content(string.Empty);

            var model = new FindPartsModel();
            model.AvailableMake = makes.Select(m => new SelectListItem() { Text = m.Name, Value = m.Name }).ToList();

            var modelCategories = await _makeTypeModelService.GetAllModelCategoriesAsync();
            model.AvailableCategories = await modelCategories.SelectAwait(async m => 
                new SelectListItem() { Text = (await _categoryService.GetCategoryByIdAsync(m.CategoryId))?.Name, Value = m.CategoryId.ToString() }).ToListAsync();

            //insert this default item at first
            model.AvailableMake.Insert(0, new SelectListItem {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Make.Hint"), Value = string.Empty, Selected = true, Disabled= true });
            model.AvailableType.Insert(0, new SelectListItem { 
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Type.Hint"), Value = string.Empty, Selected = true, Disabled = true });
            model.AvailableModel.Insert(0, new SelectListItem {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Model.Hint"), Value = string.Empty, Selected = true, Disabled = true });
            var categories = $"{await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Category.Hint")} ({await _localizationService.GetResourceAsync("Common.All")})";
            model.AvailableCategories.Insert(0, new SelectListItem { Text = categories, Value = "0", Selected = true });

            return View("~/Plugins/Widgets.MakeTypeModel/Views/FindParts.cshtml", model);
        }

        #endregion
    }
}