using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    /// <summary>
    /// Represents a charts and tables search model
    /// </summary>
    public partial record ChartsAndTablesSearchModel : BaseSearchModel
    {
        #region Ctor

        public ChartsAndTablesSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            GroupByOptions = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableRoles = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StartDate")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.StoreId")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentStatus")]
        public int PaymentStatusId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.GroupBy")]
        public int SearchGroupId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CategoryId")]
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ManufacturerId")]
        public int ManufacturerId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.VendoerId")]
        public int VendorId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.RoleId")]
        public int RoleId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.CountryId")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.ShippingMethod")]
        public string ShippingMethod { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesSearch.PaymentMethod")]
        public string PaymentMethod { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        public IList<SelectListItem> GroupByOptions { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public IList<SelectListItem> AvailableRoles { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        public IList<SelectListItem> AvailablePaymentMethods { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public bool IsChartReport { get; set; }

        public bool ProductReport { get; set; }

        public bool CategoryReport { get; set; }

        public bool ManufacturerReport { get; set; }

        public bool CustomerReport { get; set; }

        public bool VendorReport { get; set; }

        public bool OrderReport { get; set; }

        public bool TableReport { get; set; }

        #endregion

        #region Product 

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

        #region Category 

        public bool CategoryDetailCategoryReport { get; set; }

        public bool CategoryDetailProductReport { get; set; }

        public bool CategoryDetailManufacturerReport { get; set; }

        public bool CategoryDetailVendorReport { get; set; }

        public bool CategoryDetailShareCategoryReport { get; set; }

        #endregion

        #region Manufacturer 

        public bool ManufacturerDetailManufacturerReport { get; set; }

        public bool ManufacturerDetailProductReport { get; set; }

        public bool ManufacturerDetailCategoryReport { get; set; }

        public bool ManufacturerDetailShareManufacturerReport { get; set; }

        #endregion

        #region Vendor 

        public bool VendorDetailVendorReport { get; set; }

        public bool VendorDetailProductReport { get; set; }

        public bool VendorDetailCategoryReport { get; set; }

        public bool VendorDetailManufacturerReport { get; set; }

        public bool VendorDetailShareVendorReport { get; set; }

        #endregion

        #region Customer 

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

        public bool OrderMenuOrderReport { get; set; }

        public bool OrderMenuOrderItemReport { get; set; }

        public bool OrderMenuShippingMethodReport { get; set; }

        public bool OrderMenuPaymentMethodReport { get; set; }

        #endregion
    }
}
