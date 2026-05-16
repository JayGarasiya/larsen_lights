using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Reports.ChartsAndTables.Factories;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Plugin.Reports.ChartsAndTables.Security;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Reports.ChartsAndTables.Controllers
{
    public partial class ChartsAndTablesMenuController : BaseAdminController
    {
        #region Fields

        protected readonly IPermissionService _permissionService;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly ChartsAndTablesSettings _chartsAndTablesSettings;
        protected readonly IChartsAndTablesModelFactory _chartsAndTablesModelFactory;

        #endregion

        #region Ctor

        public ChartsAndTablesMenuController(IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IChartsAndTablesServices chartsAndTablesServices,
            ChartsAndTablesSettings chartsAndTablesSettings,
            IChartsAndTablesModelFactory chartsAndTablesModelFactory)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _chartsAndTablesServices = chartsAndTablesServices;
            _chartsAndTablesSettings = chartsAndTablesSettings;
            _chartsAndTablesModelFactory = chartsAndTablesModelFactory;
        }

        #endregion

        #region Dashboard

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public virtual async Task<IActionResult> ReportMenuDashboard()
        {
            return View("~/Plugins/Reports.ChartsAndTables/Views/Dashboard/DashboardReport.cshtml");
        }

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardWeekStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardWeekChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardWeekStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardMonthStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardMonthChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardMonthStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardYearStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardYearStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardYearMonthStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardMonthYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardYearMonthStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardMonthYearLastNYear,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardBestsellerProductsStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardBestSellerProductYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardBestsellerProductsYearStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardTopNChart,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardBestsellerProductsLastNMonthStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardBestSellerProductMonthChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardBestsellerProductsLastNMonthStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardTopNChart,
                _chartsAndTablesSettings.DashboardBestSellerProductLastNMonth,
                _chartsAndTablesSettings.PriceWithTax);


            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardBestsellerCategoriesStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardBestSellerCategoryYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardBestsellerCategoriesStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardTopNChart,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardBestsellerManufacturersStatistics()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE))
                return Content(string.Empty);

            if (!_chartsAndTablesSettings.DashboardBestSellerManufacturerYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardBestsellerManufacturersStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardTopNChart,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        [CheckPermission(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> LoadDashboardBestsellerVendorsStatistics()
        {
            if (!_chartsAndTablesSettings.DashboardBestSellerVendorYearChart)
                return Content(string.Empty);

            var result = await _chartsAndTablesServices.LoadDashboardBestsellerVendorsStatistics(
                _chartsAndTablesSettings.DashboardCurrentStoreOnly ? (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0 : 0,
                (await _workContext.GetCurrentVendorAsync())?.Id ?? 0,
                _chartsAndTablesSettings.DashboardReportsOrderStatus,
                _chartsAndTablesSettings.DashboardReportsPaymentStatus,
                _chartsAndTablesSettings.DashboardTopNChart,
                _chartsAndTablesSettings.PriceWithTax);

            return Json(result);
        }

        #endregion

        #region Product 
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuProducts()
        {
            var model = await _chartsAndTablesModelFactory.PrepareReportMenuProductSearchModelAsync(new ChartsAndTablesSearchModel());

            return View("~/Plugins/Reports.ChartsAndTables/Views/Product/ProductsMenuReport.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuProductList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuProductListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuProductAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuProductAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuProductAttributeList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuProductAttributeListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuProductAttributeAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuProductAttributeAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuCategoryList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuCategoryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuCategoryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuCategoryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuManufacturerList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuManufacturerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuManufacturerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuManufacturerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuVendorList(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuVendorListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportProductMenuVendorAggregates(ChartsAndTablesSearchModel searchModel)
        {
            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareProductMenuVendorAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Customer 
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuCustomers()
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            var model = await _chartsAndTablesModelFactory.PrepareReportMenuCustomerSearchModelAsync(new ChartsAndTablesSearchModel());

            return View("~/Plugins/Reports.ChartsAndTables/Views/Customer/CustomersMenuReport.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuRegisteredCustomerList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerRegisteredListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuRegisteredCustomerAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerRegisteredAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerGenderList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerGenderListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerGenderAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerGenderAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerRolesList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerRoleListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCustomerRolesAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCustomerRoleAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCountryList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCountryListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportCustomerMenuCountryAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareCustomerMenuCountryAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Order
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuOrders()
        {
            if (!await _permissionService.AuthorizeAsync(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS) &&
                !await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            var model = await _chartsAndTablesModelFactory.PrepareReportMenuOrderSearchModelAsync(new ChartsAndTablesSearchModel());

            return View("~/Plugins/Reports.ChartsAndTables/Views/Order/OrdersMenuReport.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuOrderList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuOrderListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuOrderAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuOrderAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuOrderItemList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuOrderItemListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuOrderItemAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuOrderItemAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuShippingMethodList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuShippingMethodListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuShippingMethodAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuShippingMethodAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuPaymentMethodList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuPaymentMethodListModelAsync(searchModel);

            if (searchModel.IsChartReport)
                return Json(model.Data);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportOrderMenuPaymentMethodAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareOrderMenuPaymentMethodAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Table
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuTable()
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            var model = await _chartsAndTablesModelFactory.PrepareReportMenuTableSearchModelAsync(new ChartsAndTablesSearchModel());

            return View("~/Plugins/Reports.ChartsAndTables/Views/Order/TableReport.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuTableList(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareTableReportListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ReportMenuTableAggregates(ChartsAndTablesSearchModel searchModel)
        {
            if (await _workContext.GetCurrentVendorAsync() != null)
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _chartsAndTablesModelFactory.PrepareTableReportAggregatorModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}
