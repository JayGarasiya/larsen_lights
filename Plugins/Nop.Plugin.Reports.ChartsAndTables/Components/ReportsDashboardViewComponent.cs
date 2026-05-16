using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Reports.ChartsAndTables.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class ReportsDashboardViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IStoreContext _storeContext;
        protected readonly ISettingService _settingService;
        protected readonly IPermissionService _permissionService;
        protected readonly ChartsAndTablesSettings _chartsAndTablesSettings;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public ReportsDashboardViewComponent(IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService,
            ChartsAndTablesSettings chartsAndTablesSettings,
            IChartsAndTablesServices chartsAndTablesServices,
            WidgetSettings widgetSettings)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _chartsAndTablesSettings = chartsAndTablesSettings;
            _chartsAndTablesServices = chartsAndTablesServices;
            _widgetSettings = widgetSettings;
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
            var licenseKey = _chartsAndTablesSettings.LicenseKey;
            var responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(licenseKey);
            if (responseCode != 100)
                return Content(string.Empty);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName))
                return Content(string.Empty);

            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            if (!chartsAndTablesSettings.DashboardReport)
                return Content(string.Empty);

            if(!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return Content(string.Empty);

            var model = new DashboardReportModel()
            {
                DashboardWeekChart = chartsAndTablesSettings.DashboardWeekChart,
                DashboardMonthChart = chartsAndTablesSettings.DashboardMonthChart,
                DashboardYearChart = chartsAndTablesSettings.DashboardYearChart,
                DashboardMonthYearChart = chartsAndTablesSettings.DashboardMonthYearChart,
                DashboardBestSellerProductYearChart = chartsAndTablesSettings.DashboardBestSellerProductYearChart,
                DashboardBestSellerProductMonthChart = chartsAndTablesSettings.DashboardBestSellerProductMonthChart,
                DashboardBestSellerProductLastNMonth = chartsAndTablesSettings.DashboardBestSellerProductLastNMonth,
                DashboardBestSellerCategoryYearChart = chartsAndTablesSettings.DashboardBestSellerCategoryYearChart,
                DashboardBestSellerManufacturerYearChart = chartsAndTablesSettings.DashboardBestSellerManufacturerYearChart,
                DashboardBestSellerVendorYearChart = chartsAndTablesSettings.DashboardBestSellerVendorYearChart,
                ManageProducts = await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE),
                ManageCategories = await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE),
                ManageManufacturers = await _permissionService.AuthorizeAsync(StandardPermission.Catalog.MANUFACTURER_CREATE_EDIT_DELETE),
                ManageVendors = await _permissionService.AuthorizeAsync(StandardPermission.Customers.VENDORS_CREATE_EDIT_DELETE)
            };

            return View("~/Plugins/Reports.ChartsAndTables/Views/Dashboard/DashboardChart.cshtml", model);
        }

        #endregion
    }
}