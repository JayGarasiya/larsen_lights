using Nop.Core.Configuration;

namespace Nop.Plugin.Reports.ChartsAndTables
{
    /// <summary>
    /// Represents Charts And Tables Settings
    /// </summary>
    public class ChartsAndTablesSettings : ISettings
    {
        public string LicenseKey { get; set; }

        public bool PriceWithTax { get; set; }

        public bool DashboardReport { get; set; }

        public bool ProductReport { get; set; }

        public bool CategoryReport { get; set; }

        public bool ManufacturerReport { get; set; }

        public bool CustomerReport { get; set; }

        public bool VendorReport { get; set; }

        public bool OrderReport { get; set; }

        public bool TableReport { get; set; }

        #region Admin Dashboard

        public string DashboardWidgetZone { get; set; }

        public int DashboardTopNChart { get; set; }

        public bool DashboardCurrentStoreOnly { get; set; }

        public bool DashboardWeekChart { get; set; }

        public bool DashboardMonthChart { get; set; }

        public bool DashboardYearChart { get; set; }

        public bool DashboardMonthYearChart { get; set; }

        public int DashboardMonthYearLastNYear { get; set; }

        public bool DashboardBestSellerProductYearChart { get; set; }

        public bool DashboardBestSellerProductMonthChart { get; set; }

        public int DashboardBestSellerProductLastNMonth { get; set; }

        public bool DashboardBestSellerCategoryYearChart { get; set; }

        public bool DashboardBestSellerManufacturerYearChart { get; set; }

        public bool DashboardBestSellerVendorYearChart { get; set; }

        public int DashboardReportsOrderStatus { get; set; }

        public int DashboardReportsPaymentStatus { get; set; }

        #endregion

        #region Product

        public int ProductTopNChart { get; set; }

        public bool ProductMenuProductReport { get; set; }

        public bool ProductMenuProductAttributeReport { get; set; }

        public bool ProductMenuCategoryReport { get; set; }

        public bool ProductMenuManufacturerReport { get; set; }

        public bool ProductMenuVendorReport { get; set; }

        public bool ProductDetailProductReport { get; set; }

        public bool ProductDetailCountryReport { get; set; }

        public bool ProductDetailCustomerReport { get; set; }

        public bool ProductDetailProductAttributeReport { get; set; }

        public bool ProductDetailGlobalReport { get; set; }

        #endregion

        #region Customer

        public int CustomerTopNChart { get; set; }

        public bool CustomerMenuCustomerReport { get; set; }

        public bool CustomerMenuRegisteredCustomerReport { get; set; }

        public bool CustomerMenuGenderReport { get; set; }

        public bool CustomerMenuCustomerRolesReport { get; set; }

        public bool CustomerMenuCountryReport { get; set; }

        public bool CustomerDetailCustomerReport { get; set; }

        public bool CustomerDetailProductReport { get; set; }

        public bool CustomerDetailCategoryReport { get; set; }

        public bool CustomerDetailManufacturerReport { get; set; }

        public bool CustomerDetailVendorReport { get; set; }

        #endregion

        #region Order

        public int OrderTopNRecord { get; set; }

        public bool OrderMenuOrderReport { get; set; }

        public bool OrderMenuOrderItemReport { get; set; }

        public bool OrderMenuShippingMethodReport { get; set; }

        public bool OrderMenuPaymentMethodReport { get; set; }

        #endregion

        #region Category

        public int CategoryTopNChart { get; set; }

        public bool CategoryDetailCategoryReport { get; set; }

        public bool CategoryDetailProductReport { get; set; }

        public bool CategoryDetailManufacturerReport { get; set; }

        public bool CategoryDetailVendorReport { get; set; }

        public bool CategoryDetailShareCategoryReport { get; set; }

        #endregion

        #region Manufacturer

        public int ManufacturerTopNChart { get; set; }

        public bool ManufacturerDetailManufacturerReport { get; set; }

        public bool ManufacturerDetailProductReport { get; set; }

        public bool ManufacturerDetailCategoryReport { get; set; }

        public bool ManufacturerDetailShareManufacturerReport { get; set; }

        #endregion

        #region Vendor

        public int VendorTopNChart { get; set; }

        public bool VendorDetailVendorReport { get; set; }

        public bool VendorDetailProductReport { get; set; }

        public bool VendorDetailCategoryReport { get; set; }

        public bool VendorDetailManufacturerReport { get; set; }

        public bool VendorDetailShareVendorReport { get; set; }

        #endregion
    }
}
