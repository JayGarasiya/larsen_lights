using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.MakeTypeModel.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Controllers
{
    public class FindPartsController : CatalogController
    {
        #region Fields

        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly IFindPartsModelFactory _findPartsModelFactory;

        #endregion

        #region Ctor

        public FindPartsController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IFilterLevelValueModelFactory filterLevelValueModelFactory,
            IFilterLevelValueService filterLevelValueService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INopUrlHelper nopUrlHelper,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            FilterLevelSettings filterLevelSettings,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
            IMakeTypeModelService makeTypeModelService,
            IFindPartsModelFactory findPartsModelFactory)
            : base(catalogSettings, aclService, catalogModelFactory, categoryService, customerActivityService, filterLevelValueModelFactory, filterLevelValueService, genericAttributeService, localizationService, manufacturerService, nopUrlHelper, permissionService, productModelFactory, productService, productTagService, storeContext, storeMappingService, vendorService, webHelper, workContext, filterLevelSettings, mediaSettings, vendorSettings)
        {
            _makeTypeModelService = makeTypeModelService;
            _findPartsModelFactory = findPartsModelFactory;
        }

        #endregion

        #region Methods 

        #region Searching

        ///<returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> Search(SearchModel model, CatalogProductsCommand command)
        {
            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var searchModel = new Models.SearchModel()
            {
                make_ = Request.Query.ContainsKey("make_") ? Request.Query["make_"].ToString().Trim() : string.Empty,
                type_ = Request.Query.ContainsKey("type_") ? Request.Query["type_"].ToString().Trim() : string.Empty,
                model_ = Request.Query.ContainsKey("model_") ? Request.Query["model_"].ToString().Trim() : string.Empty,
            };

            if (model == null)
                searchModel = new Models.SearchModel();
            else
            {
                searchModel.cid = model.cid;
                searchModel.CatalogProductsModel = model.CatalogProductsModel;
                searchModel.advs = true;
                searchModel.isc = true;
                searchModel.q = model.q;
                searchModel.sid = true;
            }

            searchModel = await _findPartsModelFactory.PrepareSearchModelAsync(searchModel, command);

            return View("~/Plugins/Widgets.MakeTypeModel/Views/Search.cshtml", searchModel);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SearchProducts(SearchModel searchModel, CatalogProductsCommand command)
        {
            var model = new Models.SearchModel()
            {
                make_ = Request.Query.ContainsKey("make_") ? Request.Query["make_"].ToString().Trim() : string.Empty,
                type_ = Request.Query.ContainsKey("type_") ? Request.Query["type_"].ToString().Trim() : string.Empty,
                model_ = Request.Query.ContainsKey("model_") ? Request.Query["model_"].ToString().Trim() : string.Empty,
            };

            if (searchModel == null)
                model = new Models.SearchModel();
            else
            {
                model.cid = searchModel.cid;
                model.CatalogProductsModel = searchModel.CatalogProductsModel;
                model.advs = true;
                model.isc = true;
                model.q = searchModel.q;
                model.sid = true;
            }

            var catelogProducts = await _findPartsModelFactory.PrepareSearchProductsModelAsync(model, command);

            return PartialView("~/Views/Catalog/_ProductsInGridOrLines.cshtml", catelogProducts);
        }

        #endregion

        #region Find Parts

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GetTypesByMake(string make)
        {
            if (string.IsNullOrEmpty(make))
                throw new ArgumentNullException(nameof(make));

            var result = new List<SelectListItem>();

            var modelProducts = (await _makeTypeModelService
                .GetAllModelProductsAsync(makeName: make))
                .Select(t => t.TypeName)
                .Distinct().ToList();

            if (modelProducts.Any())
            {
                var types = (await _makeTypeModelService.GetAllTypeProductsAsync())
                    .Where(t => modelProducts.Contains(t.Name))
                    .OrderBy(t => t.DisplayOrder)
                    .Select(t => t.Name)
                    .Distinct().ToList();

                foreach (var type in types)
                    result.Add(new SelectListItem { Text = type, Value = type });
            }

            result.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Type.Hint"),
                Value = string.Empty,
                Selected = true,
                Disabled = true
            });

            return Json(result);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GetModelsByMakeType(string make, string type)
        {
            if (string.IsNullOrEmpty(make) || string.IsNullOrEmpty(type))
                return Json(new List<SelectListItem>());

            var result = new List<SelectListItem>();

            var modelProducts = (await _makeTypeModelService
                .GetAllModelProductsAsync(makeName: make, typeName: type))
                .Select(t => t.Name.Trim())
                .Distinct().ToList();

            foreach (var model in modelProducts)
                result.Add(new SelectListItem { Text = model, Value = model });

            result.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Model.Hint"),
                Value = string.Empty,
                Selected = true,
                Disabled = true
            });

            return Json(result);
        }

        #endregion

        #endregion
    }
}
