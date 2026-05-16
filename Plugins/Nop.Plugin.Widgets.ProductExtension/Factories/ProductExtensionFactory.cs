using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.ProductExtension.Domain;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Widgets.ProductExtension.Factories
{
    /// <summary>
    /// Represents the product extension factory implementation
    /// </summary>
    public partial class ProductExtensionFactory : IProductExtensionFactory
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IProductExtendService _productExtendService;

        #endregion

        #region Ctor
        public ProductExtensionFactory(
            ILocalizationService localizationService,
            IProductExtendService productExtendService)
        {
            _localizationService = localizationService;
            _productExtendService = productExtendService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare product note model
        /// </summary>
        /// <param name="model">Product Note Model</param>
        /// <param name="productNote">Product Note</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store model
        /// </returns>
        public virtual async Task<ProductNoteModel> PrepareProductNoteModelAsync(ProductNoteModel model, ProductNote productNote)
        {
            if (productNote != null)
            {
                model = new ProductNoteModel()
                {
                    Id = productNote.Id,
                    Name = productNote.Name,
                    Description = productNote.Description,
                    WidgetZone = productNote.WidgetZone,
                    Published = productNote.Published,
                    DisplayOrder = productNote.DisplayOrder,
                    ShowDisplayName = productNote.ShowDisplayName
                };
            }

            //prepare available widget zones
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsAfterBreadcrumb, Text = "Product details after breadcrumb" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsTop, Text = "Product details top" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsEssentialTop, Text = "Product details essential top" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsOverviewTop, Text = "Product details overview top" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsOverviewBottom, Text = "Product details overview bottom" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsEssentialBottom, Text = "Product details essential bottom" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsBeforeCollateral, Text = "Product details before collateral" });
            model.AvailableWidgetZones.Add(new SelectListItem() { Value = PublicWidgetZones.ProductDetailsBottom, Text = "Product details bottom" });

            model.AvailableWidgetZones.Insert(0, new SelectListItem() { Value = "", Text = await _localizationService.GetResourceAsync("Admin.Common.EmptyItemText") });
            return model;
        }

        /// <summary>
        /// Prepare product notes search model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual ProductNoteSearchModel PrepareProductNotesSearchModelAsync(ProductNoteSearchModel searchModel)
        {
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product notes list model async
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public virtual async Task<ProductNoteListModel> PrepareProductNotesListModelAsync(ProductNoteSearchModel searchModel)
        {
            var productnotes = await _productExtendService.GetAllProductNotesAsync(showHidden: true, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new ProductNoteListModel().PrepareToGrid(searchModel, productnotes, () =>
            {
                //fill in model values from the entity
                return productnotes.Select(productnote => new ProductNoteModel
                {
                    Id = productnote.Id,
                    Name = productnote.Name,
                    Published = productnote.Published,
                    ShowDisplayName = productnote.ShowDisplayName,
                    DisplayOrder = productnote.DisplayOrder
                });
            });

            return model;
        }
        #endregion
    }
}
