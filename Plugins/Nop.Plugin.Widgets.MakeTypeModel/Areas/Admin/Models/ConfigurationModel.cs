using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor
        public ConfigurationModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableModelCategories = new List<SelectListItem>();
            AvailableProductCategories = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.Enable")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.HyCapacityCategoryId")]
        public int HyCapacityCategoryId { get; set; }
        public bool HyCapacityCategoryId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.HyCapacityManufacturerId")]
        public int HyCapacityManufacturerId { get; set; }
        public bool HyCapacityManufacturerId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.HyCapacityVendorId")]
        public int HyCapacityVendorId { get; set; }
        public bool HyCapacityVendorId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.HyCapacityWarehouseId")]
        public int HyCapacityWarehouseId { get; set; }
        public bool HyCapacityWarehouseId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.KitProductModelCategoryId")]
        public int KitProductModelCategoryId { get; set; }
        public bool KitProductModelCategoryId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableModelCategories { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.KitProductCategoryId")]
        public int KitProductCategoryId { get; set; }
        public bool KitProductCategoryId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableProductCategories { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.FindPartsCategoryIds")]
        public IList<int> FindPartsCategoryIds { get; set; }
        public bool FindPartsCategoryIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.FindPartsIncludeSubCategories")]
        public bool FindPartsIncludeSubCategories { get; set; }
        public bool FindPartsIncludeSubCategories_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.GranitCategoryId")]
        public int GranitCategoryId { get; set; }
        public bool GranitCategoryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.GranitVendorId")]
        public int GranitVendorId { get; set; }
        public bool GranitVendorId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.GranitWarehouseId")]
        public int GranitWarehouseId { get; set; }
        public bool GranitWarehouseId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.UpdateProductImage")]
        public bool UpdateProductImage { get; set; }
        public bool UpdateProductImage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.AuthorizationAPIURL")]
        public string AuthorizationAPIURL { get; set; }
        public bool AuthorizationAPIURL_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.ProductsAPIURL")]
        public string ProductsAPIURL { get; set; }
        public bool ProductsAPIURL_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.InventoryAPIURL")]
        public string InventoryAPIURL { get; set; }
        public bool InventoryAPIURL_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPIPagesize")]
        public int ProductUpdateAPIPagesize { get; set;}
        public bool ProductUpdateAPIPagesize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPICurrentPage")]
        public int ProductUpdateAPICurrentPage { get; set; }
        public bool ProductUpdateAPICurrentPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPINoOfPages")]
        public int ProductUpdateAPINoOfPages { get; set; }
        public bool ProductUpdateAPINoOfPages_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.LastExecutedProductAPI")]
        public DateTime? LastExecutedProductAPI { get; set; }
        public bool LastExecutedProductAPI_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.LastExecutedStockAPI")]
        public DateTime? LastExecutedStockAPI { get; set; }
        public bool LastExecutedStockAPI_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.TokenGuidId")]
        public string TokenGuidId { get; set; }
        public bool TokenGuidId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.ExcludeCategory")]
        public string ExcludeCategory { get; set; }
        public bool ExcludeCategory_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.EnabledFreeSkus")]
        public bool EnabledFreeSkus { get; set; }
        public bool EnabledFreeSkus_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.SubTotalGreaterThan")]
        public int SubTotalGreaterThan { get; set; }
        public bool SubTotalGreaterThan_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.FreeItemsSkus")]
        public string FreeItemsSkus { get; set; }
        public bool FreeItemsSkus_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Fields.FreeItemsWarehouseIds")]
        public IList<int> FreeItemsWarehouseIds { get; set; }
        public bool FreeItemsWarehouseIds_OverrideForStore { get; set; }
        #endregion
    }
}
