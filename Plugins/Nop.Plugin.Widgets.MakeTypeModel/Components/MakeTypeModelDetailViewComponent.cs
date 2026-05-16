using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.MakeTypeModel.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.MakeTypeModel.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class MakeTypeModelDetailViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IMakeTypeModelService _makeTypeModelService;

        #endregion

        #region Ctor

        public MakeTypeModelDetailViewComponent(IMakeTypeModelService makeTypeModelService)
        {
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone != MakeTypeModelDefaults.ProductDetailsAfterCollateral)
                return Content(string.Empty);

            var productId = Convert.ToInt32(additionalData);
            if(productId == 0)
                return Content(string.Empty);

            //check plugin was enable 
            if (!await _makeTypeModelService.PluginActiveAsync())
                return Content(string.Empty);

            var modelproducts = await _makeTypeModelService.GetAllModelProductsByProductIdAsync(productId);
            if (!modelproducts.Any())
                return Content(string.Empty);

            //Prepare model
            var model = new AccordionModel() { ProductId = productId };
            var makes = modelproducts.OrderBy(m => m.MakeName).GroupBy(m => m.MakeName);
            foreach (var make in makes)
            {
                var makeModel = new AccordionModel.MakeModel() { MakeName = make.Key };
                var types = make.OrderBy(t => t.TypeName).GroupBy(t => t.TypeName).Distinct();
                foreach(var type in types)
                {
                    var typeModel = new AccordionModel.MakeModel.TypeModel() { TypeName = type.Key };
                    var mproducts = type.OrderBy(m => m.Name);
                    foreach(var mproduct in mproducts)
                    {
                        typeModel.ModelsProduct.Add(new AccordionModel.MakeModel.TypeModel.ModelsProductModel()
                        {
                            Name = mproduct.Name,
                            Description = mproduct.Description,
                            QueryString = $"/search?make_={mproduct.MakeName.Trim()}&type_={mproduct.TypeName.Trim()}&model_={mproduct.Name.Trim()}"
                        });
                    }
                    makeModel.TypeModels.Add(typeModel);
                }
                model.MakeModels.Add(makeModel);
            }

            return View("~/Plugins/Widgets.MakeTypeModel/Views/PublicInfo.cshtml", model);
        }

        #endregion
    }
}