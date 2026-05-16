using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Reports.ChartsAndTables.Factories;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Reports.ChartsAndTables.Controllers
{
    public partial class ChartsAndTablesDetailController : BaseAdminController
    {
        #region Fields

        protected readonly IPermissionService _permissionService;
        protected readonly IChartsAndTablesModelFactory _chartsAndTablesModelFactory;

        #endregion

        #region Ctor

        public ChartsAndTablesDetailController(IPermissionService permissionService,
            IChartsAndTablesModelFactory chartsAndTablesModelFactory)
        {
            _permissionService = permissionService;
            _chartsAndTablesModelFactory = chartsAndTablesModelFactory;
        }

        #endregion

        #region Product 

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailProductAttributeList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailProductAttributeListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailProductAttributeAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailProductAttributeAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailCustomerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailCustomerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailCustomerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailCustomerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailCountryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailCountryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailCountryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailCountryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductDetailGlobalList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductDetailGlobalListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        #endregion

        #region Customer

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailCustomerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailCustomerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailCustomerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailCustomerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailCategoryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailCategoryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailCategoryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailCategoryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailManufacturerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailManufacturerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailManufacturerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailManufacturerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailVendorList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailVendorListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerDetailVendorAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerDetailVendorAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Category

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailCategoryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailCategoryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailCategoryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailCategoryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailManufacturerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailManufacturerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailManufacturerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailManufacturerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailVendorList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailVendorListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailVendorAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailVendorAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailCategoriesShareList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailCategoriesShareListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCategoryDetailCategoriesShareAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCategoryDetailCategoriesShareAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Manufacturer

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailManufacturerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailManufacturerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailManufacturerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailManufacturerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailCategoryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailCategoryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailCategoryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailCategoryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailManufacturersShareList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailManufacturersShareListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportManufacturerDetailManufacturersShareAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareManufacturerDetailManufacturersShareAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Vendor

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailVendorList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailVendorListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailVendorAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailVendorAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailCategoryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailCategoryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailCategoryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailCategoryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailManufacturerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailManufacturerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailManufacturerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailManufacturerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailVendorsShareList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailVendorsShareListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportVendorDetailVendorsShareAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareVendorDetailVendorsShareAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}
