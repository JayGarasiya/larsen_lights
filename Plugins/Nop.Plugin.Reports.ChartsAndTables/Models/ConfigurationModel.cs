using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            AvailableWidgetZones = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.LicenseKey")]
        public string LicenseKey { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.Enable")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.PriceWithTax")]
        public bool PriceWithTax { get; set; }
        public bool PriceWithTax_OverrideForStore { get; set; }

        #region Admin Dashboard

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReport")]
        public bool DashboardReport { get; set; }
        public bool DashboardReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardTopNChart")]
        public int DashboardTopNChart { get; set; }
        public bool DashboardTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWidgetZone")]
        public string DashboardWidgetZone { get; set; }
        public bool DashboardWidgetZone_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardCurrentStoreOnly")]
        public bool DashboardCurrentStoreOnly { get; set; }
        public bool DashboardCurrentStoreOnly_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardWeekChart")]
        public bool DashboardWeekChart { get; set; }
        public bool DashboardWeekChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthChart")]
        public bool DashboardMonthChart { get; set; }
        public bool DashboardMonthChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardYearChart")]
        public bool DashboardYearChart { get; set; }
        public bool DashboardYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearChart")]
        public bool DashboardMonthYearChart { get; set; }
        public bool DashboardMonthYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardMonthYearLastNYear")]
        public int DashboardMonthYearLastNYear { get; set; }
        public bool DashboardMonthYearLastNYear_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductYearChart")]
        public bool DashboardBestSellerProductYearChart { get; set; }
        public bool DashboardBestSellerProductYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductMonthChart")]
        public bool DashboardBestSellerProductMonthChart { get; set; }
        public bool DashboardBestSellerProductMonthChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerProductLastNMonth")]
        public int DashboardBestSellerProductLastNMonth { get; set; }
        public bool DashboardBestSellerProductLastNMonth_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerCategoryYearChart")]
        public bool DashboardBestSellerCategoryYearChart { get; set; }
        public bool DashboardBestSellerCategoryYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerManufacturerYearChart")]
        public bool DashboardBestSellerManufacturerYearChart { get; set; }
        public bool DashboardBestSellerManufacturerYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardBestSellerVendorYearChart")]
        public bool DashboardBestSellerVendorYearChart { get; set; }
        public bool DashboardBestSellerVendorYearChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsOrderStatus")]
        public int DashboardReportsOrderStatus { get; set; }
        public bool DashboardReportsOrderStatus_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.DashboardReportsPaymentStatus")]
        public int DashboardReportsPaymentStatus { get; set; }
        public bool DashboardReportsPaymentStatus_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        #endregion

        #region Product

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductReport")]
        public bool ProductReport { get; set; }
        public bool ProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductTopNChart")]
        public int ProductTopNChart { get; set; }
        public bool ProductTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductReport")]
        public bool ProductMenuProductReport { get; set; }
        public bool ProductMenuProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuProductAttributeReport")]
        public bool ProductMenuProductAttributeReport { get; set; }
        public bool ProductMenuProductAttributeReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuCategoryReport")]
        public bool ProductMenuCategoryReport { get; set; }
        public bool ProductMenuCategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuManufacturerReport")]
        public bool ProductMenuManufacturerReport { get; set; }
        public bool ProductMenuManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductMenuVendorReport")]
        public bool ProductMenuVendorReport { get; set; }
        public bool ProductMenuVendorReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductReport")]
        public bool ProductDetailProductReport { get; set; }
        public bool ProductDetailProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCountryReport")]
        public bool ProductDetailCountryReport { get; set; }
        public bool ProductDetailCountryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailCustomerReport")]
        public bool ProductDetailCustomerReport { get; set; }
        public bool ProductDetailCustomerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailProductAttributeReport")]
        public bool ProductDetailProductAttributeReport { get; set; }
        public bool ProductDetailProductAttributeReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ProductDetailGlobalReport")]
        public bool ProductDetailGlobalReport { get; set; }
        public bool ProductDetailGlobalReport_OverrideForStore { get; set; }

        #endregion

        #region Category

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryReport")]
        public bool CategoryReport { get; set; }
        public bool CategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryTopNChart")]
        public int CategoryTopNChart { get; set; }
        public bool CategoryTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailCategoryReport")]
        public bool CategoryDetailCategoryReport { get; set; }
        public bool CategoryDetailCategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailProductReport")]
        public bool CategoryDetailProductReport { get; set; }
        public bool CategoryDetailProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailManufacturerReport")]
        public bool CategoryDetailManufacturerReport { get; set; }
        public bool CategoryDetailManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailVendorReport")]
        public bool CategoryDetailVendorReport { get; set; }
        public bool CategoryDetailVendorReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CategoryDetailShareCategoryReport")]
        public bool CategoryDetailShareCategoryReport { get; set; }
        public bool CategoryDetailShareCategoryReport_OverrideForStore { get; set; }

        #endregion

        #region Manufacturer

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerReport")]
        public bool ManufacturerReport { get; set; }
        public bool ManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerTopNChart")]
        public int ManufacturerTopNChart { get; set; }
        public bool ManufacturerTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailManufacturerReport")]
        public bool ManufacturerDetailManufacturerReport { get; set; }
        public bool ManufacturerDetailManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailProductReport")]
        public bool ManufacturerDetailProductReport { get; set; }
        public bool ManufacturerDetailProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailCategoryReport")]
        public bool ManufacturerDetailCategoryReport { get; set; }
        public bool ManufacturerDetailCategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.ManufacturerDetailShareManufacturerReport")]
        public bool ManufacturerDetailShareManufacturerReport { get; set; }
        public bool ManufacturerDetailShareManufacturerReport_OverrideForStore { get; set; }

        #endregion

        #region Customer

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerReport")]
        public bool CustomerReport { get; set; }
        public bool CustomerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerTopNChart")]
        public int CustomerTopNChart { get; set; }
        public bool CustomerTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerReport")]
        public bool CustomerMenuCustomerReport { get; set; }
        public bool CustomerMenuCustomerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuRegisteredCustomerReport")]
        public bool CustomerMenuRegisteredCustomerReport { get; set; }
        public bool CustomerMenuRegisteredCustomerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuGenderReport")]
        public bool CustomerMenuGenderReport { get; set; }
        public bool CustomerMenuGenderReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCustomerRolesReport")]
        public bool CustomerMenuCustomerRolesReport { get; set; }
        public bool CustomerMenuCustomerRolesReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerMenuCountryReport")]
        public bool CustomerMenuCountryReport { get; set; }
        public bool CustomerMenuCountryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCustomerReport")]
        public bool CustomerDetailCustomerReport { get; set; }
        public bool CustomerDetailCustomerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailProductReport")]
        public bool CustomerDetailProductReport { get; set; }
        public bool CustomerDetailProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailCategoryReport")]
        public bool CustomerDetailCategoryReport { get; set; }
        public bool CustomerDetailCategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailManufacturerReport")]
        public bool CustomerDetailManufacturerReport { get; set; }
        public bool CustomerDetailManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.CustomerDetailVendorReport")]
        public bool CustomerDetailVendorReport { get; set; }
        public bool CustomerDetailVendorReport_OverrideForStore { get; set; }

        #endregion

        #region Vendor

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorReport")]
        public bool VendorReport { get; set; }
        public bool VendorReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorTopNChart")]
        public int VendorTopNChart { get; set; }
        public bool VendorTopNChart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailVendorReport")]
        public bool VendorDetailVendorReport { get; set; }
        public bool VendorDetailVendorReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailProductReport")]
        public bool VendorDetailProductReport { get; set; }
        public bool VendorDetailProductReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailCategoryReport")]
        public bool VendorDetailCategoryReport { get; set; }
        public bool VendorDetailCategoryReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailManufacturerReport")]
        public bool VendorDetailManufacturerReport { get; set; }
        public bool VendorDetailManufacturerReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.VendorDetailShareVendorReport")]
        public bool VendorDetailShareVendorReport { get; set; }
        public bool VendorDetailShareVendorReport_OverrideForStore { get; set; }

        #endregion

        #region Order

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderReport")]
        public bool OrderReport { get; set; }
        public bool OrderReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderTopNRecord")]
        public int OrderTopNRecord { get; set; }
        public bool OrderTopNRecord_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderReport")]
        public bool OrderMenuOrderReport { get; set; }
        public bool OrderMenuOrderReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuOrderItemReport")]
        public bool OrderMenuOrderItemReport { get; set; }
        public bool OrderMenuOrderItemReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuShippingMethodReport")]
        public bool OrderMenuShippingMethodReport { get; set; }
        public bool OrderMenuShippingMethodReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.OrderMenuPaymentMethodReport")]
        public bool OrderMenuPaymentMethodReport { get; set; }
        public bool OrderMenuPaymentMethodReport_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Configuration.Fields.TableReport")]
        public bool TableReport { get; set; }
        public bool TableReport_OverrideForStore { get; set; }

        #endregion

        #endregion
    }
}
