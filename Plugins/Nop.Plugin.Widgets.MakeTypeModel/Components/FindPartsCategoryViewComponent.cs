using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.MakeTypeModel.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class FindPartsCategoryViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IFindPartsModelFactory _findPartsModelFactory;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly ICategoryService _categoryService;
        protected readonly IStoreContext _storeContext;
        protected readonly IMakeTypeModelService _makeTypeModelService;

        #endregion

        #region Ctor

        public FindPartsCategoryViewComponent(IFindPartsModelFactory findPartsModelFactory,
            MakeTypeModelSettings makeTypeModelSettings,
            ICategoryService categoryService,
            IStoreContext storeContext,
            IMakeTypeModelService makeTypeModelService)
        {
            _findPartsModelFactory = findPartsModelFactory;
            _makeTypeModelSettings = makeTypeModelSettings;
            _categoryService = categoryService;
            _storeContext = storeContext;
            _makeTypeModelService = makeTypeModelService;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            if (widgetZone != PublicWidgetZones.CategoryDetailsTop)
                return Content(string.Empty);

            if (!(additionalData is CategoryModel categoryModel))
                return Content(string.Empty);

            //check plugin was enable 
            if (!await _makeTypeModelService.PluginActiveAsync())
                return Content(string.Empty);

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var categoryIds = new List<int>();
            categoryIds.AddRange(_makeTypeModelSettings.FindPartsCategoryIds);
            if(_makeTypeModelSettings.FindPartsIncludeSubCategories)
            {
                //include subcategories
                _makeTypeModelSettings.FindPartsCategoryIds.ForEach(async fc => 
                    categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(fc, currentStore.Id)));
            }

            if(!categoryIds.Any(c=> c.Equals(categoryModel.Id)))
                return Content(string.Empty);

            var model = await _findPartsModelFactory.PrepareSearchModelAsync(new Models.SearchModel(), new CatalogProductsCommand());

            return View("~/Plugins/Widgets.MakeTypeModel/Views/FindPartsCategory.cshtml", model);
        }

        #endregion
    }
}