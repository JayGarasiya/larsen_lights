using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Plugin.Reports.ChartsAndTables.Security;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Reports.ChartsAndTables.Controllers
{
    public class ChartsAndTablesController : BaseAdminController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IWebHelper _webHelper;
        protected readonly IStoreContext _storeContext;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly INotificationService _notificationService;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly ILogger _logger;

        #endregion

        #region Ctor

        public ChartsAndTablesController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            IChartsAndTablesServices chartsAndTablesServices,
            INotificationService notificationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILogger logger)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _chartsAndTablesServices = chartsAndTablesServices;
            _notificationService = notificationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _logger = logger;
        }

        #endregion

        #region Methods

        #region Configuration
        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public async Task<IActionResult> MiscConfigure()
        {
            return View("~/Plugins/Reports.ChartsAndTables/Views/Configuration/MiscConfigure.cshtml");
        }

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>(storeScope);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeScope);

            //If license key not exist then redirect to license page.
            if (string.IsNullOrEmpty(chartsAndTablesSettings.LicenseKey))
                return RedirectToAction("LicenseKey");

            try
            {
                var responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(chartsAndTablesSettings.LicenseKey);
                switch (responseCode)
                {
                    case 100: // perfact
                        break;

                    case 101: // trial mode
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.TrialMode");
                        break;

                    case 102: //domain mismatch
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.DomainMismatch");
                        return RedirectToAction("LicenseKey");

                    case 103: // license key expire
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.LicenseExpire");
                        return RedirectToAction("LicenseKey");

                    default: //Invalid license keys. Please check your license keys.
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");
                        return RedirectToAction("LicenseKey");
                }
            }
            catch (Exception ex)
            { 
                // Invalid license keys. Please check your license keys.
                await _logger.ErrorAsync(ex.Message, ex);
                ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");
                return RedirectToAction("LicenseKey");
            }

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName),
                PriceWithTax = chartsAndTablesSettings.PriceWithTax,
                DashboardReport = chartsAndTablesSettings.DashboardReport,
                DashboardTopNChart = chartsAndTablesSettings.DashboardTopNChart,
                DashboardWidgetZone = chartsAndTablesSettings.DashboardWidgetZone,
                DashboardCurrentStoreOnly = chartsAndTablesSettings.DashboardCurrentStoreOnly,
                DashboardWeekChart = chartsAndTablesSettings.DashboardWeekChart,
                DashboardMonthChart = chartsAndTablesSettings.DashboardMonthChart,
                DashboardYearChart = chartsAndTablesSettings.DashboardYearChart,
                DashboardMonthYearChart = chartsAndTablesSettings.DashboardMonthYearChart,
                DashboardMonthYearLastNYear = chartsAndTablesSettings.DashboardMonthYearLastNYear,
                DashboardBestSellerProductYearChart = chartsAndTablesSettings.DashboardBestSellerProductYearChart,
                DashboardBestSellerProductMonthChart = chartsAndTablesSettings.DashboardBestSellerProductMonthChart,
                DashboardBestSellerProductLastNMonth = chartsAndTablesSettings.DashboardBestSellerProductLastNMonth,
                DashboardBestSellerCategoryYearChart = chartsAndTablesSettings.DashboardBestSellerCategoryYearChart,
                DashboardBestSellerManufacturerYearChart = chartsAndTablesSettings.DashboardBestSellerManufacturerYearChart,
                DashboardBestSellerVendorYearChart = chartsAndTablesSettings.DashboardBestSellerVendorYearChart,
                DashboardReportsOrderStatus = chartsAndTablesSettings.DashboardReportsOrderStatus,
                DashboardReportsPaymentStatus = chartsAndTablesSettings.DashboardReportsPaymentStatus,
                ProductReport = chartsAndTablesSettings.ProductReport,
                ProductTopNChart = chartsAndTablesSettings.ProductTopNChart,
                ProductMenuProductReport = chartsAndTablesSettings.ProductMenuProductReport,
                ProductMenuProductAttributeReport = chartsAndTablesSettings.ProductMenuProductAttributeReport,
                ProductMenuCategoryReport = chartsAndTablesSettings.ProductMenuCategoryReport,
                ProductMenuManufacturerReport = chartsAndTablesSettings.ProductMenuManufacturerReport,
                ProductMenuVendorReport = chartsAndTablesSettings.ProductMenuVendorReport,
                ProductDetailProductReport = chartsAndTablesSettings.ProductDetailProductReport,
                ProductDetailCountryReport = chartsAndTablesSettings.ProductDetailCountryReport,
                ProductDetailCustomerReport = chartsAndTablesSettings.ProductDetailCustomerReport,
                ProductDetailProductAttributeReport = chartsAndTablesSettings.ProductDetailProductAttributeReport,
                ProductDetailGlobalReport = chartsAndTablesSettings.ProductDetailGlobalReport,
                CategoryReport = chartsAndTablesSettings.CategoryReport,
                CategoryTopNChart = chartsAndTablesSettings.CategoryTopNChart,
                CategoryDetailCategoryReport = chartsAndTablesSettings.CategoryDetailCategoryReport,
                CategoryDetailProductReport = chartsAndTablesSettings.CategoryDetailProductReport,
                CategoryDetailManufacturerReport = chartsAndTablesSettings.CategoryDetailManufacturerReport,
                CategoryDetailVendorReport = chartsAndTablesSettings.CategoryDetailVendorReport,
                CategoryDetailShareCategoryReport = chartsAndTablesSettings.CategoryDetailShareCategoryReport,
                ManufacturerReport = chartsAndTablesSettings.ManufacturerReport,
                ManufacturerTopNChart = chartsAndTablesSettings.ManufacturerTopNChart,
                ManufacturerDetailManufacturerReport = chartsAndTablesSettings.ManufacturerDetailManufacturerReport,
                ManufacturerDetailProductReport = chartsAndTablesSettings.ManufacturerDetailProductReport,
                ManufacturerDetailCategoryReport = chartsAndTablesSettings.ManufacturerDetailCategoryReport,
                ManufacturerDetailShareManufacturerReport = chartsAndTablesSettings.ManufacturerDetailShareManufacturerReport,
                CustomerReport = chartsAndTablesSettings.CustomerReport,
                CustomerTopNChart = chartsAndTablesSettings.CustomerTopNChart,
                CustomerMenuCustomerReport = chartsAndTablesSettings.CustomerMenuCustomerReport,
                CustomerMenuRegisteredCustomerReport = chartsAndTablesSettings.CustomerMenuRegisteredCustomerReport,
                CustomerMenuGenderReport = chartsAndTablesSettings.CustomerMenuGenderReport,
                CustomerMenuCustomerRolesReport = chartsAndTablesSettings.CustomerMenuCustomerRolesReport,
                CustomerMenuCountryReport = chartsAndTablesSettings.CustomerMenuCountryReport,
                CustomerDetailCustomerReport = chartsAndTablesSettings.CustomerDetailCustomerReport,
                CustomerDetailProductReport = chartsAndTablesSettings.CustomerDetailProductReport,
                CustomerDetailCategoryReport = chartsAndTablesSettings.CustomerDetailCategoryReport,
                CustomerDetailManufacturerReport = chartsAndTablesSettings.CustomerDetailManufacturerReport,
                CustomerDetailVendorReport = chartsAndTablesSettings.CustomerDetailVendorReport,
                VendorReport = chartsAndTablesSettings.VendorReport,
                VendorTopNChart = chartsAndTablesSettings.VendorTopNChart,
                VendorDetailVendorReport = chartsAndTablesSettings.VendorDetailVendorReport,
                VendorDetailProductReport = chartsAndTablesSettings.VendorDetailProductReport,
                VendorDetailCategoryReport = chartsAndTablesSettings.VendorDetailCategoryReport,
                VendorDetailManufacturerReport = chartsAndTablesSettings.VendorDetailManufacturerReport,
                VendorDetailShareVendorReport = chartsAndTablesSettings.VendorDetailShareVendorReport,
                OrderReport = chartsAndTablesSettings.OrderReport,
                OrderTopNRecord = chartsAndTablesSettings.OrderTopNRecord,
                OrderMenuOrderReport = chartsAndTablesSettings.OrderMenuOrderReport,
                OrderMenuOrderItemReport = chartsAndTablesSettings.OrderMenuOrderItemReport,
                OrderMenuShippingMethodReport = chartsAndTablesSettings.OrderMenuShippingMethodReport,
                OrderMenuPaymentMethodReport = chartsAndTablesSettings.OrderMenuPaymentMethodReport,
                TableReport = chartsAndTablesSettings.TableReport
            };

            #region Prepare available order statuses, payment statuses and Widget Zones

            await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses);
            if (model.AvailableOrderStatuses.Any())
            {
                if (model.DashboardReportsOrderStatus> 0)
                {
                    model.AvailableOrderStatuses.Where(statusItem => statusItem.Value == model.DashboardReportsOrderStatus.ToString()).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    model.AvailableOrderStatuses.FirstOrDefault().Selected = true;
            }

            await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses);
            if (model.AvailablePaymentStatuses.Any())
            {
                if (model.DashboardReportsPaymentStatus > 0)
                {
                    model.AvailablePaymentStatuses.Where(statusItem => statusItem.Value == model.DashboardReportsPaymentStatus.ToString()).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    model.AvailablePaymentStatuses.FirstOrDefault().Selected = true;
            }

            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard top", Value = AdminWidgetZones.DashboardTop });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard after news", Value = AdminWidgetZones.DashboardNewsAfter });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard after commonstatistics", Value = AdminWidgetZones.DashboardCommonstatisticsAfter });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard after customer order charts", Value = AdminWidgetZones.DashboardCustomerorderchartsAfter });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard after order reports", Value = AdminWidgetZones.DashboardOrderreportsAfter });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard after latest orders & search terms", Value = AdminWidgetZones.DashboardLatestordersSearchtermsAfter });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Admin dashboard bottom", Value = AdminWidgetZones.DashboardBottom });
            model.AvailableWidgetZones.Add(new SelectListItem { Text = "Reports dashboard", Value = ChartsAndTablesDefaults.ReportsDashboard });

            #endregion

            if (storeScope > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeScope);
                model.PriceWithTax_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.PriceWithTax, storeScope);
                model.DashboardReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardReport, storeScope);
                model.DashboardTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardTopNChart, storeScope);
                model.DashboardWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardWidgetZone, storeScope);
                model.DashboardCurrentStoreOnly_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardCurrentStoreOnly, storeScope);
                model.DashboardWeekChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardWeekChart, storeScope);
                model.DashboardMonthChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardMonthChart, storeScope);
                model.DashboardYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardYearChart, storeScope);
                model.DashboardMonthYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardMonthYearChart, storeScope);
                model.DashboardMonthYearLastNYear_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardMonthYearLastNYear, storeScope);
                model.DashboardBestSellerProductYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerProductYearChart, storeScope);
                model.DashboardBestSellerProductMonthChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerProductMonthChart, storeScope);
                model.DashboardBestSellerProductLastNMonth_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerProductLastNMonth, storeScope);
                model.DashboardBestSellerCategoryYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerCategoryYearChart, storeScope);
                model.DashboardBestSellerManufacturerYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerManufacturerYearChart, storeScope);
                model.DashboardBestSellerVendorYearChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardBestSellerVendorYearChart, storeScope);
                model.DashboardReportsOrderStatus_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardReportsOrderStatus, storeScope);
                model.DashboardReportsPaymentStatus_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.DashboardReportsPaymentStatus, storeScope);
                model.ProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductReport, storeScope);
                model.ProductTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductTopNChart, storeScope);
                model.ProductMenuProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductMenuProductReport, storeScope);
                model.ProductMenuProductAttributeReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductMenuProductAttributeReport, storeScope);
                model.ProductMenuCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductMenuCategoryReport, storeScope);
                model.ProductMenuManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductMenuManufacturerReport, storeScope);
                model.ProductMenuVendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductMenuVendorReport, storeScope);
                model.ProductDetailProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductDetailProductReport, storeScope);
                model.ProductDetailCountryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductDetailCountryReport, storeScope);
                model.ProductDetailCustomerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductDetailCustomerReport, storeScope);
                model.ProductDetailProductAttributeReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductDetailProductAttributeReport, storeScope);
                model.ProductDetailGlobalReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ProductDetailGlobalReport, storeScope);
                model.CategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryReport, storeScope);
                model.CategoryTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryTopNChart, storeScope);
                model.CategoryDetailCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryDetailCategoryReport, storeScope);
                model.CategoryDetailProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryDetailProductReport, storeScope);
                model.CategoryDetailManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryDetailManufacturerReport, storeScope);
                model.CategoryDetailVendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryDetailVendorReport, storeScope);
                model.CategoryDetailShareCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CategoryDetailShareCategoryReport, storeScope);
                model.ManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerReport, storeScope);
                model.ManufacturerTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerTopNChart, storeScope);
                model.ManufacturerDetailManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerDetailManufacturerReport, storeScope);
                model.ManufacturerDetailProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerDetailProductReport, storeScope);
                model.ManufacturerDetailCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerDetailCategoryReport, storeScope);
                model.ManufacturerDetailShareManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.ManufacturerDetailShareManufacturerReport, storeScope);
                model.CustomerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerReport, storeScope);
                model.CustomerTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerTopNChart, storeScope);
                model.CustomerMenuCustomerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerMenuCustomerReport, storeScope);
                model.CustomerMenuRegisteredCustomerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerMenuRegisteredCustomerReport, storeScope);
                model.CustomerMenuGenderReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerMenuGenderReport, storeScope);
                model.CustomerMenuCustomerRolesReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerMenuCustomerRolesReport, storeScope);
                model.CustomerMenuCountryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerMenuCountryReport, storeScope);
                model.CustomerDetailCustomerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerDetailCustomerReport, storeScope);
                model.CustomerDetailProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerDetailProductReport, storeScope);
                model.CustomerDetailCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerDetailCategoryReport, storeScope);
                model.CustomerDetailManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerDetailManufacturerReport, storeScope);
                model.CustomerDetailVendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.CustomerDetailVendorReport, storeScope);
                model.VendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorReport, storeScope);
                model.VendorTopNChart_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorTopNChart, storeScope);
                model.VendorDetailVendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorDetailVendorReport, storeScope);
                model.VendorDetailProductReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorDetailProductReport, storeScope);
                model.VendorDetailCategoryReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorDetailCategoryReport, storeScope);
                model.VendorDetailManufacturerReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorDetailManufacturerReport, storeScope);
                model.VendorDetailShareVendorReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.VendorDetailShareVendorReport, storeScope);
                model.OrderReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderReport, storeScope);
                model.OrderTopNRecord_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderTopNRecord, storeScope);
                model.OrderMenuOrderReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderMenuOrderReport, storeScope);
                model.OrderMenuOrderItemReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderMenuOrderItemReport, storeScope);
                model.OrderMenuShippingMethodReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderMenuShippingMethodReport, storeScope);
                model.OrderMenuPaymentMethodReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.OrderMenuPaymentMethodReport, storeScope);
                model.TableReport_OverrideForStore = await _settingService.SettingExistsAsync(chartsAndTablesSettings, setting => setting.TableReport, storeScope);
            }

            return View("~/Plugins/Reports.ChartsAndTables/Views/Configuration/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        [AutoValidateAntiforgeryToken]
        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>(storeScope);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeScope);
            
            //save settings
            chartsAndTablesSettings.PriceWithTax = model.PriceWithTax;
            chartsAndTablesSettings.DashboardReport = model.DashboardReport;
            chartsAndTablesSettings.DashboardTopNChart = model.DashboardTopNChart;
            chartsAndTablesSettings.DashboardWidgetZone = model.DashboardWidgetZone;
            chartsAndTablesSettings.DashboardCurrentStoreOnly = model.DashboardCurrentStoreOnly;
            chartsAndTablesSettings.DashboardWeekChart = model.DashboardWeekChart;
            chartsAndTablesSettings.DashboardMonthChart = model.DashboardMonthChart;
            chartsAndTablesSettings.DashboardYearChart = model.DashboardYearChart;
            chartsAndTablesSettings.DashboardMonthYearChart = model.DashboardMonthYearChart;
            chartsAndTablesSettings.DashboardMonthYearLastNYear = model.DashboardMonthYearLastNYear;
            chartsAndTablesSettings.DashboardBestSellerProductYearChart = model.DashboardBestSellerProductYearChart;
            chartsAndTablesSettings.DashboardBestSellerProductMonthChart = model.DashboardBestSellerProductMonthChart;
            chartsAndTablesSettings.DashboardBestSellerProductLastNMonth = model.DashboardBestSellerProductLastNMonth;
            chartsAndTablesSettings.DashboardBestSellerCategoryYearChart = model.DashboardBestSellerCategoryYearChart;
            chartsAndTablesSettings.DashboardBestSellerManufacturerYearChart = model.DashboardBestSellerManufacturerYearChart;
            chartsAndTablesSettings.DashboardBestSellerVendorYearChart = model.DashboardBestSellerVendorYearChart;
            chartsAndTablesSettings.DashboardReportsOrderStatus = model.DashboardReportsOrderStatus;
            chartsAndTablesSettings.DashboardReportsPaymentStatus = model.DashboardReportsPaymentStatus;
            chartsAndTablesSettings.ProductReport = model.ProductReport;
            chartsAndTablesSettings.ProductTopNChart = model.ProductTopNChart;
            chartsAndTablesSettings.ProductMenuProductReport = model.ProductMenuProductReport;
            chartsAndTablesSettings.ProductMenuProductAttributeReport = model.ProductMenuProductAttributeReport;
            chartsAndTablesSettings.ProductMenuCategoryReport = model.ProductMenuCategoryReport;
            chartsAndTablesSettings.ProductMenuManufacturerReport = model.ProductMenuManufacturerReport;
            chartsAndTablesSettings.ProductMenuVendorReport = model.ProductMenuVendorReport;
            chartsAndTablesSettings.ProductDetailProductReport = model.ProductDetailProductReport;
            chartsAndTablesSettings.ProductDetailCountryReport = model.ProductDetailCountryReport;
            chartsAndTablesSettings.ProductDetailCustomerReport = model.ProductDetailCustomerReport;
            chartsAndTablesSettings.ProductDetailProductAttributeReport = model.ProductDetailProductAttributeReport;
            chartsAndTablesSettings.ProductDetailGlobalReport = model.ProductDetailGlobalReport;
            chartsAndTablesSettings.CategoryReport = model.CategoryReport;
            chartsAndTablesSettings.CategoryTopNChart = model.CategoryTopNChart;
            chartsAndTablesSettings.CategoryDetailCategoryReport = model.CategoryDetailCategoryReport;
            chartsAndTablesSettings.CategoryDetailProductReport = model.CategoryDetailProductReport;
            chartsAndTablesSettings.CategoryDetailManufacturerReport = model.CategoryDetailManufacturerReport;
            chartsAndTablesSettings.CategoryDetailVendorReport = model.CategoryDetailVendorReport;
            chartsAndTablesSettings.CategoryDetailShareCategoryReport = model.CategoryDetailShareCategoryReport;
            chartsAndTablesSettings.ManufacturerReport = model.ManufacturerReport;
            chartsAndTablesSettings.ManufacturerTopNChart = model.ManufacturerTopNChart;
            chartsAndTablesSettings.ManufacturerDetailManufacturerReport = model.ManufacturerDetailManufacturerReport;
            chartsAndTablesSettings.ManufacturerDetailProductReport = model.ManufacturerDetailProductReport;
            chartsAndTablesSettings.ManufacturerDetailCategoryReport = model.ManufacturerDetailCategoryReport;
            chartsAndTablesSettings.ManufacturerDetailShareManufacturerReport = model.ManufacturerDetailShareManufacturerReport;
            chartsAndTablesSettings.CustomerReport = model.CustomerReport;
            chartsAndTablesSettings.CustomerTopNChart = model.CustomerTopNChart;
            chartsAndTablesSettings.CustomerMenuCustomerReport = model.CustomerMenuCustomerReport;
            chartsAndTablesSettings.CustomerMenuRegisteredCustomerReport = model.CustomerMenuRegisteredCustomerReport;
            chartsAndTablesSettings.CustomerMenuGenderReport = model.CustomerMenuGenderReport;
            chartsAndTablesSettings.CustomerMenuCustomerRolesReport = model.CustomerMenuCustomerRolesReport;
            chartsAndTablesSettings.CustomerMenuCountryReport = model.CustomerMenuCountryReport;
            chartsAndTablesSettings.CustomerDetailCustomerReport = model.CustomerDetailCustomerReport;
            chartsAndTablesSettings.CustomerDetailProductReport = model.CustomerDetailProductReport;
            chartsAndTablesSettings.CustomerDetailCategoryReport = model.CustomerDetailCategoryReport;
            chartsAndTablesSettings.CustomerDetailManufacturerReport = model.CustomerDetailManufacturerReport;
            chartsAndTablesSettings.CustomerDetailVendorReport = model.CustomerDetailVendorReport;
            chartsAndTablesSettings.VendorReport = model.VendorReport;
            chartsAndTablesSettings.VendorTopNChart = model.VendorTopNChart;
            chartsAndTablesSettings.VendorDetailVendorReport = model.VendorDetailVendorReport;
            chartsAndTablesSettings.VendorDetailProductReport = model.VendorDetailProductReport;
            chartsAndTablesSettings.VendorDetailCategoryReport = model.VendorDetailCategoryReport;
            chartsAndTablesSettings.VendorDetailManufacturerReport = model.VendorDetailManufacturerReport;
            chartsAndTablesSettings.VendorDetailShareVendorReport = model.VendorDetailShareVendorReport;
            chartsAndTablesSettings.OrderReport = model.OrderReport;
            chartsAndTablesSettings.OrderTopNRecord = model.OrderTopNRecord;
            chartsAndTablesSettings.OrderMenuOrderReport = model.OrderMenuOrderReport;
            chartsAndTablesSettings.OrderMenuOrderItemReport = model.OrderMenuOrderItemReport;
            chartsAndTablesSettings.OrderMenuShippingMethodReport = model.OrderMenuShippingMethodReport;
            chartsAndTablesSettings.OrderMenuPaymentMethodReport = model.OrderMenuPaymentMethodReport;
            chartsAndTablesSettings.TableReport = model.TableReport;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.PriceWithTax, model.PriceWithTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardReport, model.DashboardReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardReport, model.DashboardTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardTopNChart, model.DashboardTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardWidgetZone, model.DashboardWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardCurrentStoreOnly, model.DashboardCurrentStoreOnly_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardWeekChart, model.DashboardWeekChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardMonthChart, model.DashboardMonthChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardYearChart, model.DashboardYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardMonthYearChart, model.DashboardMonthYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardMonthYearLastNYear, model.DashboardMonthYearLastNYear_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerProductYearChart, model.DashboardBestSellerProductYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerProductMonthChart, model.DashboardBestSellerProductMonthChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerProductLastNMonth, model.DashboardBestSellerProductLastNMonth_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerCategoryYearChart, model.DashboardBestSellerCategoryYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerManufacturerYearChart, model.DashboardBestSellerManufacturerYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardBestSellerVendorYearChart, model.DashboardBestSellerVendorYearChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardReportsOrderStatus, model.DashboardReportsOrderStatus_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.DashboardReportsPaymentStatus, model.DashboardReportsPaymentStatus_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductReport, model.ProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductTopNChart, model.ProductTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductMenuProductReport, model.ProductMenuProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductMenuProductAttributeReport, model.ProductMenuProductAttributeReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductMenuCategoryReport, model.ProductMenuCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductMenuManufacturerReport, model.ProductMenuManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductMenuVendorReport, model.ProductMenuVendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductDetailProductReport, model.ProductDetailProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductDetailCountryReport, model.ProductDetailCountryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductDetailCustomerReport, model.ProductDetailCustomerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductDetailProductAttributeReport, model.ProductDetailProductAttributeReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ProductDetailGlobalReport, model.ProductDetailGlobalReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryReport, model.CategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryTopNChart, model.CategoryTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryDetailCategoryReport, model.CategoryDetailCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryDetailProductReport, model.CategoryDetailProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryDetailManufacturerReport, model.CategoryDetailManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryDetailVendorReport, model.CategoryDetailVendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CategoryDetailShareCategoryReport, model.CategoryDetailShareCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerReport, model.ManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerTopNChart, model.ManufacturerTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerDetailManufacturerReport, model.ManufacturerDetailManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerDetailProductReport, model.ManufacturerDetailProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerDetailCategoryReport, model.ManufacturerDetailCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.ManufacturerDetailShareManufacturerReport, model.ManufacturerDetailShareManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerReport, model.CustomerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerTopNChart, model.CustomerTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerMenuCustomerReport, model.CustomerMenuCustomerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerMenuRegisteredCustomerReport, model.CustomerMenuRegisteredCustomerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerMenuGenderReport, model.CustomerMenuGenderReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerMenuCustomerRolesReport, model.CustomerMenuCustomerRolesReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerMenuCountryReport, model.CustomerMenuCountryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerDetailCustomerReport, model.CustomerDetailCustomerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerDetailProductReport, model.CustomerDetailProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerDetailCategoryReport, model.CustomerDetailCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerDetailManufacturerReport, model.CustomerDetailManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.CustomerDetailVendorReport, model.CustomerDetailVendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorReport, model.VendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorTopNChart, model.VendorTopNChart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorDetailVendorReport, model.VendorDetailVendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorDetailProductReport, model.VendorDetailProductReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorDetailCategoryReport, model.VendorDetailCategoryReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorDetailManufacturerReport, model.VendorDetailManufacturerReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.VendorDetailShareVendorReport, model.VendorDetailShareVendorReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderReport, model.OrderReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderTopNRecord, model.OrderTopNRecord_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderMenuOrderReport, model.OrderMenuOrderReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderMenuOrderItemReport, model.OrderMenuOrderItemReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderMenuShippingMethodReport, model.OrderMenuShippingMethodReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.OrderMenuPaymentMethodReport, model.OrderMenuPaymentMethodReport_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chartsAndTablesSettings, x => x.TableReport, model.TableReport_OverrideForStore, storeScope, false);

            if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Add(ChartsAndTablesDefaults.SystemName);
            if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Remove(ChartsAndTablesDefaults.SystemName);
            await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeScope, true);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region LicenseKey

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public async Task<IActionResult> LicenseKey(string licenseKey = "")
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>(storeScope);

            var model = new ConfigurationModel();
            model.LicenseKey = chartsAndTablesSettings.LicenseKey;

            //save settings
            try
            {
                if (string.IsNullOrEmpty(licenseKey) && string.IsNullOrEmpty(model.LicenseKey))
                {
                    ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.EnterLicensekey");
                    return View("~/Plugins/Reports.ChartsAndTables/Views/Configuration/LicenseKey.cshtml", model);
                }

                var responseCode = 0;
                if (!string.IsNullOrEmpty(licenseKey))
                    responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(licenseKey);
                else
                    responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(model.LicenseKey);

                switch (responseCode)
                {
                    case 100: // perfact
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        return RedirectToAction("Configure");

                    case 101: // Your plugin is in trial mode.
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.TrialMode");
                        return RedirectToAction("Configure");

                    case 102: //Your domain is missmatched with this license keys.
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.DomainMismatch");
                        break;

                    case 103: //Your trial license is expired
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.LicenseExpire");
                        break;

                    default: // Invalid license keys. Please check your license keys.
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");
                        break;
                }
            }
            catch (Exception ex)
            {   // Invalid license keys. Please check your license keys.
                await _logger.InsertLogAsync(LogLevel.Warning, ex.Message.ToString(), ex.StackTrace.ToString());
                ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");
            }

            return View("~/Plugins/Reports.ChartsAndTables/Views/Configuration/LicenseKey.cshtml", model);
        }

        [HttpPost, ActionName("LicenseKey")]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        [CheckPermission(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS)]
        public async Task<IActionResult> LicenseKey(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>(storeScope);

            //save settings
            chartsAndTablesSettings.LicenseKey = model.LicenseKey;

            //Check for Licensing  and save settings
            try
            {
                var responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(model.LicenseKey);
                switch (responseCode)
                {
                    case 100: // perfact
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        return RedirectToAction("Configure");

                    case 101: // Your plugin is in trial mode.
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.TrialMode");
                        return RedirectToAction("Configure");

                    case 102: //Your domain is missmatched with this license keys.
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.DomainMismatch");
                        return await LicenseKey();

                    case 103: //Your trial license is expired
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.LicenseExpire");
                        return await LicenseKey();

                    default: // Invalid license keys. Please check your license keys.
                        await _settingService.SaveSettingAsync(chartsAndTablesSettings);
                        ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");
                        return await LicenseKey(model.LicenseKey);
                }
            }
            catch (Exception ex)
            {   // Invalid license keys. Please check your license keys.
                await _logger.InsertLogAsync(LogLevel.Warning, ex.Message.ToString(), ex.StackTrace.ToString());
                ViewBag.Warning = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense");

                return View("~/Plugins/Reports.ChartsAndTables/Views/Configuration/LicenseKey.cshtml", model);
            }
        }

        #endregion

        #endregion
    }
}
