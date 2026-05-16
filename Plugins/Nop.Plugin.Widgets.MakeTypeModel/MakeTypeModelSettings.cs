using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.MakeTypeModel
{
    /// <summary>
    /// Represents Make, Type, and Model widget settings.
    /// </summary>
    public class MakeTypeModelSettings : ISettings
    {
        public MakeTypeModelSettings()
        {
            WidgetZone = new List<string>();
            FindPartsCategoryIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the widget zones where the plugin will be displayed.
        /// </summary>
        public List<string> WidgetZone { get; set; }

        /// <summary>
        /// Gets or sets the Hy-Capacity category identifier.
        /// </summary>
        public int HyCapacityCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Hy-Capacity manufacturer identifier.
        /// </summary>
        public int HyCapacityManufacturerId { get; set; }

        /// <summary>
        /// Gets or sets the Hy-Capacity vendor identifier.
        /// </summary>
        public int HyCapacityVendorId { get; set; }

        /// <summary>
        /// Gets or sets the Hy-Capacity warehouse identifier.
        /// </summary>
        public int HyCapacityWarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the Granit category identifier.
        /// </summary>
        public int GranitCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Granit vendor identifier.
        /// </summary>
        public int GranitVendorId { get; set; }

        /// <summary>
        /// Gets or sets the Granit warehouse identifier.
        /// </summary>
        public int GranitWarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the number of Hy-Capacity model fit records to process.
        /// </summary>
        public int HyCapacityModelFitsRecords { get; set; }

        /// <summary>
        /// Gets or sets the number of Hy-Capacity product records to process.
        /// </summary>
        public int HyCapacityProductsRecords { get; set; }

        /// <summary>
        /// Gets or sets the kit product model category identifier.
        /// </summary>
        public int KitProductModelCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the kit product category identifier.
        /// </summary>
        public int KitProductCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category identifiers used for finding parts.
        /// </summary>
        public List<int> FindPartsCategoryIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subcategories should be included when finding parts.
        /// </summary>
        public bool FindPartsIncludeSubCategories { get; set; }

        /// <summary>
        /// Gets or sets the number of Granit product records to process.
        /// </summary>
        public int GraintProductsRecords { get; set; }

        /// <summary>
        /// Gets or sets the number of price import records to process.
        /// </summary>
        public int PriceImportRecords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new file has been imported.
        /// </summary>
        public bool ImportedNewFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to update product image.
        /// </summary>
        public bool UpdateProductImage { get; set; }

        /// <summary>
        /// Gets or sets the authorization API URL.
        /// </summary>
        public string AuthorizationAPIURL { get; set; }

        /// <summary>
        /// Gets or sets the products API URL.
        /// </summary>
        public string ProductsAPIURL { get; set; }

        /// <summary>
        /// Gets or sets the inventory API URL.
        /// </summary>
        public string InventoryAPIURL { get; set; }

        /// <summary>
        /// Gets or sets the API username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the API password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the product update API page size.
        /// </summary>
        public int ProductUpdateAPIPagesize { get; set; }

        /// <summary>
        /// Gets or sets the current product update API page number.
        /// </summary>
        public int ProductUpdateAPICurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of product update API pages.
        /// </summary>
        public int ProductUpdateAPINoOfPages { get; set; }

        /// <summary>
        /// Gets or sets the token GUID identifier.
        /// </summary>
        public string TokenGuidId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the stock API was last executed (UTC).
        /// </summary>
        public DateTime? LastExecutedStockAPI { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the product API was last executed (UTC).
        /// </summary>
        public DateTime? LastExecutedProductAPI { get; set; }

        /// <summary>
        /// Gets or sets the categories to exclude from processing.
        /// </summary>
        public string ExcludeCategory { get; set; }

        /// <summary>
        /// Gets or sets enable to add free item.
        /// </summary>
        public bool EnabledFreeSkus { get; set; }

        /// <summary>
        /// Gets or sets the sub total amount to add free item.
        /// </summary>
        public int SubTotalGreaterThan { get; set; }

        /// <summary>
        /// Gets or sets the product skus to add free item.
        /// </summary>
        public string FreeItemsSkus { get; set; }

        /// <summary>
        /// Gets or sets the warehouses to add free item.
        /// </summary>
        public List<int> FreeItemsWarehouseIds { get; set; }
    }
}
