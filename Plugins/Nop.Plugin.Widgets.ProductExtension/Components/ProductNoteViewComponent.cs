using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class ProductNoteViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;

        #endregion

        #region Ctor

        public ProductNoteViewComponent(IProductExtendService productExtendService)
        {
            _productExtendService = productExtendService;
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
            if (!await _productExtendService.PluginActiveAsync())
                return Content(string.Empty);

            //get product note by widget zone
            var productNotes = await _productExtendService.GetAllProductNotesAsync(widgetZone: widgetZone);

            if (!productNotes.Any())
                return Content(string.Empty);

            //prepare model
            var model = productNotes.Select(note => new PublicInfoModel()
            {
                Name = note.Name,
                Description = note.Description,
                ShowDisplayName = note.ShowDisplayName
            }).ToList();

            return View("~/Plugins/Widgets.ProductExtension/Views/PublicInfo.cshtml", model);
        }

        #endregion
    }
}