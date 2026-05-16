using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Security;
using Nop.Plugin.Reports.ChartsAndTables.Components;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Reports.ChartsAndTables
{
    /// <summary>
    /// Represents the Charts And Tables plugin
    /// </summary>
    public class ChartsAndTablesPlugin : BasePlugin, IWidgetPlugin
    {
        #region Fields
        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;
        protected readonly ChartsAndTablesSettings _chartsAndTablesSettings;
        protected readonly IPermissionService _permissionService;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly IWorkContext _workContext;
        protected readonly ISettingService _settingService;
        protected readonly WidgetSettings _widgetSettings;
        protected readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public ChartsAndTablesPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ChartsAndTablesSettings chartsAndTablesSettings,
            IPermissionService permissionService,
            IChartsAndTablesServices chartsAndTablesServices,
            IWorkContext workContext,
            ISettingService settingService,
            WidgetSettings widgetSettings,
            ICustomerService customerService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _chartsAndTablesSettings = chartsAndTablesSettings;
            _permissionService = permissionService;
            _chartsAndTablesServices = chartsAndTablesServices;
            _workContext = workContext;
            _settingService = settingService;
            _widgetSettings = widgetSettings;
            _customerService = customerService;
        }
        #endregion

        #region Utilities
        protected virtual async Task InstallLocaleResourcesAsync()
        {
            //locals
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Reports.ChartsAndTables"] = "Reports - Chart and Table",
                ["Plugins.Reports.ChartsAndTables.License"] = "Reports - Chart and Table License",
                ["Plugins.Reports.ChartsAndTables.Configuration"] = "Configuration",

                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey"] = "License key",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.Hint"] = "Enter license key to enable plugin use.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.TrialMode"] = "Your Plugin Is In Trial Mode",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.DomainMismatch"] = "Your Domain Is Missmatched With This License Key",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.LicenseExpire"] = "Your Algolia Core Plugin Is Expire",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.InvalidLicense"] = "Invalid license key. Please check your license key",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey.EnterLicensekey"] = "Please enter your License key. If you don't have Licence key please contact to forefrontinfotech at info@forefrontinfotech.com",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.General"] = "General",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.Enable"] = "Enable",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.Enable.Hint"] = "Set to enable reports plugin.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.PriceWithTax"] = "Price with tax",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.PriceWithTax.Hint"] = "Enable to show price with tax.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Dashboard"] = "Dashboard reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReport"] = "Dashboard report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReport.Hint"] = "Select to enable dashboard reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardTopNChart"] = "Top-N-Chart for dashboard reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardTopNChart.Hint"] = "Set to Top-N-Chart for dashboard reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWidgetZone"] = "Dashboard widget zone",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWidgetZone.Hint"] = "Select dashboard widget zone to show charts.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardCurrentStoreOnly"] = "Dashboard current store only",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardCurrentStoreOnly.Hint"] = "Select dashboard current store only to load data for current active store.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWeekChart"] = "Weeks statistic",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWeekChart.Hint"] = "Set dashboard weeks statistics.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthChart"] = "Months statistics",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthChart.Hint"] = "Set dashboard months statistics.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardYearChart"] = "Years statistics",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardYearChart.Hint"] = "Set dashboard years statistics.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearChart"] = "Years and Months statistics",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearChart.Hint"] = "Set dashboard Years and Months statistics.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearLastNYear"] = "Last 'N years",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearLastNYear.Hint"] = "Enter last 'N years.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductYearChart"] = "Bestsellers products of year",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductYearChart.Hint"] = "Set dashboard bestsellers products of year.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductMonthChart"] = "Bestsellers products from last 'N months",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductMonthChart.Hint"] = "Set dashboard bestsellers products from last 'N months.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductLastNMonth"] = "Last 'N months",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductLastNMonth.Hint"] = "Enter dashboard bestsellers products last 'N months.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerCategoryYearChart"] = "Bestsellers categories",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerCategoryYearChart.Hint"] = "Set dashboard bestsellers categories.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerManufacturerYearChart"] = "Bestsellers manufacturers",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerManufacturerYearChart.Hint"] = "Set dashboard bestsellers manufacturers.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerVendorYearChart"] = "Bestsellers vendors",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerVendorYearChart.Hint"] = "Set dashboard bestsellers vendors.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsOrderStatus"] = "Dashboard with order status",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsOrderStatus.Hint"] = "The charts on the dashboard with order status.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsPaymentStatus"] = "Dashboard with payment status",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsPaymentStatus.Hint"] = "The charts on the dashboard with payment status.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Product"] = "Product reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Product.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Product.MenuReports"] = "Menu report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Product.DetailReports"] = "Detail report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductReport"] = "Product report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductReport.Hint"] = "Select to enable product report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductTopNChart"] = "Top-N-Chart for product reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductTopNChart.Hint"] = "Set to Top-N-Chart for product reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductReport"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductReport.Hint"] = "Enbale report by product.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductAttributeReport"] = "Report by product attributes",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductAttributeReport.Hint"] = "Enable report by product attributes.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuCategoryReport"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuCategoryReport.Hint"] = "Enable report by category.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuManufacturerReport"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuManufacturerReport.Hint"] = "Enable report by manufacturer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuVendorReport"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuVendorReport.Hint"] = "Enable report by vendor.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductReport"] = "Report by product in time",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductReport.Hint"] = "Enbale report by product in time.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCountryReport"] = "Report by country",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCountryReport.Hint"] = "Enable report by country.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCustomerReport"] = "Report by customer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCustomerReport.Hint"] = "Enable report by customer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductAttributeReport"] = "Report by attribute combination",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductAttributeReport.Hint"] = "Enable report by attribute combination.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailGlobalReport"] = "Global sales",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailGlobalReport.Hint"] = "Enable Global sales.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Category"] = "Category reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Category.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Category.DetailReports"] = "Detail report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryReport"] = "Category report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryReport.Hint"] = "Select to enable category report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryTopNChart"] = "Top-N-Chart for category reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryTopNChart.Hint"] = "Set to Top-N-Chart for category reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailCategoryReport"] = "Report by category in time",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailCategoryReport.Hint"] = "Enbale report by category in time.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailProductReport"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailProductReport.Hint"] = "Enable report by product.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailManufacturerReport"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailManufacturerReport.Hint"] = "Enable report by manufacturer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailVendorReport"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailVendorReport.Hint"] = "Enable report by vendor.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailShareCategoryReport"] = "Share of categories",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailShareCategoryReport.Hint"] = "Enable share of categories.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Manufacturer"] = "Manufacturer reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Manufacturer.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Manufacturer.DetailReports"] = "Detail report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerReport"] = "Manufacturer report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerReport.Hint"] = "Select to enable manufacturer report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerTopNChart"] = "Top-N-Chart for manufacturer reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerTopNChart.Hint"] = "Set to Top-N-Chart for manufacturer reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailManufacturerReport"] = "Report by manufacturer in time",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailManufacturerReport.Hint"] = "Enbale report by manufacturer in time.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailProductReport"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailProductReport.Hint"] = "Enable report by product.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailCategoryReport"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailCategoryReport.Hint"] = "Enable report by category.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailShareManufacturerReport"] = "Share of manufacturer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailShareManufacturerReport.Hint"] = "Enable share of manufacturer.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Customer"] = "Customer reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Customer.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Customer.MenuReports"] = "Menu report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Customer.DetailReports"] = "Detail report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerReport"] = "Customer report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerReport.Hint"] = "Select to enable customer report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerTopNChart"] = "Top-N-Chart for customer reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerTopNChart.Hint"] = "Set to Top-N-Chart for customer reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerReport"] = "Report by customer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerReport.Hint"] = "Enbale report by customer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuRegisteredCustomerReport"] = "Sales and registered customers",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuRegisteredCustomerReport.Hint"] = "Enable sales and registered customers.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuGenderReport"] = "Report by gender",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuGenderReport.Hint"] = "Enable report by gender.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerRolesReport"] = "Report by customer roles",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerRolesReport.Hint"] = "Enable report by customer roles.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCountryReport"] = "Report by country",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCountryReport.Hint"] = "Enable report by country.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCustomerReport"] = "Report by customer in time",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCustomerReport.Hint"] = "Enbale report by customer in time.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailProductReport"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailProductReport.Hint"] = "Enable report by product.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCategoryReport"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCategoryReport.Hint"] = "Enable report by category.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailManufacturerReport"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailManufacturerReport.Hint"] = "Enable report by manufacturer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailVendorReport"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailVendorReport.Hint"] = "Enable report by vendor.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Vendor"] = "Vendor reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Vendor.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Vendor.DetailReports"] = "Detail report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorReport"] = "Vendor report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorReport.Hint"] = "Select to enable vendor report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorTopNChart"] = "Top-N-Chart for vendor reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorTopNChart.Hint"] = "Set to Top-N-Chart for vendor reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailVendorReport"] = "Report by vendor in time",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailVendorReport.Hint"] = "Enbale report by vendor in time.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailProductReport"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailProductReport.Hint"] = "Enable report by product.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailCategoryReport"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailCategoryReport.Hint"] = "Enable report by category.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailManufacturerReport"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailManufacturerReport.Hint"] = "Enable report by manufacturer.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailShareVendorReport"] = "Share of vendors",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailShareVendorReport.Hint"] = "Enable share of vendors.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Order"] = "Order reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Order.CommonInfo"] = "Common settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Tab.Order.MenuReports"] = "Menu report settings",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderReport"] = "Order report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderReport.Hint"] = "Select to enable order report.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderTopNRecord"] = "Top-N-Records for order reports",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderTopNRecord.Hint"] = "Set to Top-N-Records for order reports.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderReport"] = "Report by order",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderReport.Hint"] = "Enbale report by order.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderItemReport"] = "Report by order item",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderItemReport.Hint"] = "Enbale report by order item.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuShippingMethodReport"] = "Report by shipping method",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuShippingMethodReport.Hint"] = "Enable report by shipping method.",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuPaymentMethodReport"] = "Report by payment method",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuPaymentMethodReport.Hint"] = "Enable report by payment method.",

                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.TableReport"] = "Table report",
                ["Plugins.Reports.ChartsAndTables.Configuration.Fields.TableReport.Hint"] = "Select to enable table report.",

                ["Plugins.Reports.ChartsAndTables.Generate"] = "Generate report",

                ["Plugins.Reports.ChartsAndTables.Dashboard"] = "Dashboard reports",
                ["Plugins.Reports.ChartsAndTables.Dashboard.Day"] = "Day",
                ["Plugins.Reports.ChartsAndTables.Dashboard.Current"] = "Current",
                ["Plugins.Reports.ChartsAndTables.Dashboard.Previous"] = "Previous",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardWeekStatistics"] = "Weeks statistics",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardMonthStatistics"] = "Months statistics",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardYearStatistics"] = "Years statistics",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardYearMonthStatistics"] = "Years and Months statistics",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardBestsellerProductsStatistics"] = "Bestsellers products of year",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardBestsellerProductsLastNMonthStatistics"] = "Bestsellers products of last {0} months",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardBestsellerCategoriesStatistics"] = "Bestsellers categories",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardBestsellerManufacturersStatistics"] = "Bestsellers manufacturers",
                ["Plugins.Reports.ChartsAndTables.Dashboard.DashboardBestsellerVendorsStatistics"] = "Bestsellers vendors",

                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StartDate"] = "Start date",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StartDate.Hint"] = "The start date for the search.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.EndDate"] = "End date",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.EndDate.Hint"] = "The end date for the search.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StoreId"] = "Store",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StoreId.Hint"] = "Filter report by orders placed in a specific store.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.OrderStatus"] = "Order status",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.OrderStatus.Hint"] = "Search by a specific order status e.g. Complete.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentStatus"] = "Payment status",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentStatus.Hint"] = "Search by a specific payment status e.g. Paid.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.GroupBy"] = "Group by",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.GroupBy.Hint"] = "Grouping by time periods.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ProductId"] = "Product",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ProductId.Hint"] = "Search in a specific product.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CustomerId"] = "Customer",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CustomerId.Hint"] = "Search in a specific customer.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CategoryId"] = "Category",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CategoryId.Hint"] = "Search in a specific category.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ManufacturerId"] = "Manufacturer",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ManufacturerId.Hint"] = "Search in a specific manufacturer.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.VendoerId"] = "Vendor",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.VendoerId.Hint"] = "Search by a specific vendor.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.RoleId"] = "Customer role",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.RoleId.Hint"] = "Filter by customer role.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CountryId"] = "Country sales",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CountryId.Hint"] = "Filter by order country.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ShippingMethod"] = "Shipping statuses",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ShippingMethod.Hint"] = "Search by a specific shipping status e.g. Not yet shipped.",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentMethod"] = "Payment method",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentMethod.Hint"] = "Search by a specific payment method.",

                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.Title"] = "Name",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.TotalSales"] = "Sales value",

                ["Plugins.Reports.ChartsAndTables.Aggrerator.Summary"] = "Summary",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.TotalSalesValue"] = "Total",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.AverageSalesValue"] = "Average",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.MinSalesValue"] = "Min",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.MaxSalesValue"] = "Max",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.TotalQuantity"] = "Total",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.AverageQuantity"] = "Average",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.MinQuantity"] = "Min",
                ["Plugins.Reports.ChartsAndTables.Aggrerator.MaxQuantity"] = "Max",

                ["Plugins.Reports.ChartsAndTables.Products"] = "Products report",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.None"] = "Please enable any product reports. Go to Configuration page and active any product menu reports.",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Product"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Product.TitleChart"] = "Sales report by product",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Product.Title"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Product.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.ProductAttribute"] = "Report by product attributes",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.ProductAttribute.TitleChart"] = "Sales report by product attributes",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.ProductAttribute.Title"] = "Product name (with attributes)",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.ProductAttribute.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.ProductAttribute.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Category"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Category.TitleChart"] = "Sales report by category",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Category.Title"] = "Category name",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Category.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Manufacturer"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Manufacturer.TitleChart"] = "Sales report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Manufacturer.Title"] = "Manufacturer name",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Manufacturer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Vendor"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Vendor.TitleChart"] = "Sales report by vendor",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Vendor.Title"] = "Vendor name",
                ["Plugins.Reports.ChartsAndTables.Products.Menu.Vendor.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Product"] = "Report by product in time",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Product.TitleChart"] = "Sales report by product in time",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Product.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Product.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.ProductAttributeCombination"] = "Report by attributes combination",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.ProductAttributeCombination.TitleChart"] = "Sales report by attributes combination",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.ProductAttributeCombination.Title"] = "Attributes combination",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.ProductAttributeCombination.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.ProductAttributeCombination.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Customer"] = "Report by customer",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Customer.TitleChart"] = "Sales report by customer",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Customer.Title"] = "Customer",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Customer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Customer.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Country"] = "Report by country",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Country.TitleChart"] = "Sales report by country",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Country.Title"] = "Country",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Country.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Country.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global"] = "Global sales",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.TitleChart"] = "Global sales report",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.Title"] = "Global",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.TotalSales"] = "Sales",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.Sales"] = "Total sales",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.Quantity"] = "Total quantity",
                ["Plugins.Reports.ChartsAndTables.Products.Detail.Global.Orders"] = "Number of orders",

                ["Plugins.Reports.ChartsAndTables.Customers"] = "Customers report",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.None"] = "Please enable any customer reports. Go to Configuration page and active any customer menu reports.",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Customer"] = "Report by customer",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Customer.TitleChart"] = "Sales report by customer",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Customer.Title"] = "Customer",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Customer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Customer.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.RegisteredCustomer"] = "Report by sales and registered customers",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.RegisteredCustomer.TitleChart"] = "Report by sales and registered customers",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.RegisteredCustomer.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.RegisteredCustomer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.RegisteredCustomer.Quantity"] = "Registered customers",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerGender"] = "Report by gender",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerGender.TitleChart"] = "Sales report by gender",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerGender.Title"] = "Gender",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerGender.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerRoles"] = "Report by customer roles",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerRoles.TitleChart"] = "Sales report by customer roles",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerRoles.Title"] = "Customer role",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerRoles.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.CustomerRoles.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Country"] = "Report by country",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Country.TitleChart"] = "Sales report by country",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Country.Title"] = "Country",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Country.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Menu.Country.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Customer"] = "Report by customer in time",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Customer.TitleChart"] = "Sales report by customer in time",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Customer.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Customer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Customer.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Product"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Product.TitleChart"] = "Sales report by product",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Product.Title"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Product.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Category"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Category.TitleChart"] = "Sales report by category",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Category.Title"] = "Category name",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Category.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Category.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Manufacturer"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Manufacturer.TitleChart"] = "Sales report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Manufacturer.Title"] = "Manufacturer name",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Manufacturer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Manufacturer.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Vendor"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Vendor.TitleChart"] = "Sales report by vendor",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Vendor.Title"] = "Vendor name",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Vendor.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Customers.Detail.Vendor.Quantity"] = "Number of orders",

                ["Plugins.Reports.ChartsAndTables.Orders"] = "Orders report",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.None"] = "Please enable any order reports. Go to Configuration page and active any order menu reports.",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.Order"] = "Report by sales in time (Number of orders.)",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.Order.TitleChart"] = "Report by sales in time",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.Order.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.Order.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.Order.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.OrderItem"] = "Report by sales in time (Number of items.)",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.OrderItem.TitleChart"] = "Report by sales in time",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.OrderItem.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.OrderItem.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.OrderItem.Quantity"] = "Number of items",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.ShippingMethod"] = "Report by shipping method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.ShippingMethod.TitleChart"] = "Report by shipping method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.ShippingMethod.Title"] = "Shipping method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.ShippingMethod.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.ShippingMethod.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.PaymentMethod"] = "Report by payment method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.PaymentMethod.TitleChart"] = "Report by payment method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.PaymentMethod.Title"] = "Payment method",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.PaymentMethod.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Orders.Menu.PaymentMethod.Quantity"] = "Number of orders",

                ["Plugins.Reports.ChartsAndTables.Table"] = "Table",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.None"] = "Please enable any table reports. Go to Configuration page and active any table reports.",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.OrderId"] = "Order#",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.CustomerId"] = "Customer",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.ProductId"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.Sku"] = "Sku",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.Quantity"] = "Qty",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.UnitPriceExclTax"] = "Price - ExclTax",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.LineTotalExclTax"] = "Line total",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.PaymentMethod"] = "Payment method",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.ShippingMethod"] = "Shipping method",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.BillingCountry"] = "Country",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.BillingCity"] = "City",
                ["Plugins.Reports.ChartsAndTables.Table.Menu.YearMonth"] = "Year & Month",

                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Category"] = "Report by category in time",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Category.TitleChart"] = "Sales report by category in time",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Category.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Category.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Product"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Product.TitleChart"] = "Sales report by product",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Product.Title"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Product.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Manufacturer"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Manufacturer.TitleChart"] = "Sales report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Manufacturer.Title"] = "Manufacturer name",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Manufacturer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Manufacturer.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Vendor"] = "Report by vendor",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Vendor.TitleChart"] = "Sales report by vendor",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Vendor.Title"] = "Vendor name",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Vendor.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.Vendor.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.CategoriesShare"] = "Share of categories",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.CategoriesShare.TitleChart"] = "Share of sales by categories",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.CategoriesShare.Title"] = "Category name",
                ["Plugins.Reports.ChartsAndTables.Categories.Detail.CategoriesShare.TotalSales"] = "Total sales value",

                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Manufacturer"] = "Report by manufacturer in time",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Manufacturer.TitleChart"] = "Sales report by manufacturer in time",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Manufacturer.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Manufacturer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Product"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Product.TitleChart"] = "Sales report by product",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Product.Title"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Product.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Category"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Category.TitleChart"] = "Sales report by category",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Category.Title"] = "Category name",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Category.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.Category.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.ManufacturersShare"] = "Share of manufacturers",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.ManufacturersShare.TitleChart"] = "Share of sales by manufacturers",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.ManufacturersShare.Title"] = "Manufacturer name",
                ["Plugins.Reports.ChartsAndTables.Manufacturers.Detail.ManufacturersShare.TotalSales"] = "Total sales value",

                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Vendor"] = "Report by vendor in time",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Vendor.TitleChart"] = "Sales report by vendor in time",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Vendor.Title"] = "Period",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Vendor.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Product"] = "Report by product",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Product.TitleChart"] = "Sales report by product",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Product.Title"] = "Product name",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Product.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Product.Quantity"] = "Quantity",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Category"] = "Report by category",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Category.TitleChart"] = "Sales report by category",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Category.Title"] = "Category name",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Category.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Category.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Manufacturer"] = "Report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Manufacturer.TitleChart"] = "Sales report by manufacturer",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Manufacturer.Title"] = "Manufacturer name",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Manufacturer.TotalSales"] = "Total sales value",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.Manufacturer.Quantity"] = "Number of orders",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.VendorsShare"] = "Share of vendors",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.VendorsShare.TitleChart"] = "Share of sales by vendors",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.VendorsShare.Title"] = "Vendor name",
                ["Plugins.Reports.ChartsAndTables.Vendors.Detail.VendorsShare.TotalSales"] = "Total sales value",
            });
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = new List<string> {
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.CategoryDetailsBlock,
                AdminWidgetZones.ManufacturerDetailsBlock,
                AdminWidgetZones.CustomerDetailsBlock,
                AdminWidgetZones.VendorDetailsBlock,
                ChartsAndTablesDefaults.ReportsDashboard
            };

            if (!string.IsNullOrEmpty(_chartsAndTablesSettings.DashboardWidgetZone))
                widgetZones.Add(_chartsAndTablesSettings.DashboardWidgetZone);

            return Task.FromResult<IList<string>>(widgetZones);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ChartsAndTables/MiscConfigure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
                return typeof(ReportsProductViewComponent);
            else if (widgetZone.Equals(AdminWidgetZones.CategoryDetailsBlock))
                return typeof(ReportsCategoryViewComponent);
            else if (widgetZone.Equals(AdminWidgetZones.ManufacturerDetailsBlock))
                return typeof(ReportsManufacturerViewComponent);
            else if (widgetZone.Equals(AdminWidgetZones.CustomerDetailsBlock))
                return typeof(ReportsCustomerViewComponent);
            else if (widgetZone.Equals(AdminWidgetZones.VendorDetailsBlock))
                return typeof(ReportsVendorViewComponent);
            else
                return typeof(ReportsDashboardViewComponent);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new ChartsAndTablesSettings
            {
                PriceWithTax = false,
                DashboardReport = true,
                DashboardTopNChart = 10,
                DashboardWidgetZone = AdminWidgetZones.DashboardTop,
                DashboardCurrentStoreOnly = true,
                DashboardWeekChart = true,
                DashboardMonthChart = true,
                DashboardYearChart = true,
                DashboardMonthYearChart = true,
                DashboardMonthYearLastNYear = 5,
                DashboardBestSellerProductYearChart = true,
                DashboardBestSellerProductMonthChart = true,
                DashboardBestSellerProductLastNMonth = 3,
                DashboardBestSellerCategoryYearChart = true,
                DashboardBestSellerManufacturerYearChart = true,
                DashboardBestSellerVendorYearChart = true,
                DashboardReportsOrderStatus = 0,
                DashboardReportsPaymentStatus = 0,
                ProductReport = true,
                ProductTopNChart = 10,
                ProductMenuProductReport = true,
                ProductMenuProductAttributeReport = true,
                ProductMenuCategoryReport = true,
                ProductMenuManufacturerReport = true,
                ProductMenuVendorReport = true,
                ProductDetailProductReport = true,
                ProductDetailCountryReport = true,
                ProductDetailCustomerReport = true,
                ProductDetailProductAttributeReport = true,
                ProductDetailGlobalReport = true,
                CategoryReport = true,
                CategoryTopNChart = 10,
                CategoryDetailCategoryReport = true,
                CategoryDetailProductReport = true,
                CategoryDetailManufacturerReport = true,
                CategoryDetailVendorReport = true,
                CategoryDetailShareCategoryReport = true,
                ManufacturerReport = true,
                ManufacturerTopNChart = 10,
                ManufacturerDetailManufacturerReport = true,
                ManufacturerDetailProductReport = true,
                ManufacturerDetailCategoryReport = true,
                ManufacturerDetailShareManufacturerReport = true,
                CustomerReport = true,
                CustomerTopNChart = 10,
                CustomerMenuCustomerReport = true,
                CustomerMenuRegisteredCustomerReport = true,
                CustomerMenuGenderReport = true,
                CustomerMenuCustomerRolesReport = true,
                CustomerMenuCountryReport = true,
                CustomerDetailCustomerReport = true,
                CustomerDetailProductReport = true,
                CustomerDetailCategoryReport = true,
                CustomerDetailManufacturerReport = true,
                CustomerDetailVendorReport = true,
                VendorReport = true,
                VendorTopNChart = 10,
                VendorDetailVendorReport = true,
                VendorDetailProductReport = true,
                VendorDetailCategoryReport = true,
                VendorDetailManufacturerReport = true,
                VendorDetailShareVendorReport = true,
                OrderReport = true,
                OrderTopNRecord = 10,
                OrderMenuOrderReport = true,
                OrderMenuOrderItemReport = true,
                OrderMenuShippingMethodReport = true,
                OrderMenuPaymentMethodReport = true,
                TableReport = true
            });

            //locales
            await InstallLocaleResourcesAsync();

            //add permissions
            var isExist = (await _permissionService.GetAllPermissionRecordsAsync()).Where(c => c.SystemName == "AccessAdminReports").FirstOrDefault() != null;
            if (!isExist)
            {
                var record = new PermissionRecord()
                {
                    Name = "Access admin. Access reports",
                    SystemName = "AccessAdminReports",
                    Category = "Configuration"
                };
                await _permissionService.InsertPermissionRecordAsync(record);
            }

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ChartsAndTablesSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Reports.ChartsAndTables");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Update plugin
        /// </summary>
        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            //locales
            await InstallLocaleResourcesAsync();
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
        #endregion
    }
}
