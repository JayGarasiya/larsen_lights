using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.ProductExtension.Domain;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Product notes service interface
    /// </summary>
    public partial interface IProductExtendService
    {
        /// <summary>
        /// Check whether the plugin is active for the current customer and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<bool> PluginActiveAsync();

        /// <summary>
        /// Deletes a Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductNoteAsync(ProductNote productNote);

        /// <summary>
        /// Gets all Product notes
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rates
        /// </returns>
        Task<IPagedList<ProductNote>> GetAllProductNotesAsync(string widgetZone = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all Product note widget zones
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        Task<IList<string>> GetAllProductNoteWidgetsAsync();

        /// <summary>
        /// Gets a Product note
        /// </summary>
        /// <param name="productNoteId">Product note identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rate
        /// </returns>
        Task<ProductNote> GetProductNoteByIdAsync(int productNoteId);

        /// <summary>
        /// Inserts a Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductNoteAsync(ProductNote productNote);

        /// <summary>
        /// Updates the Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductNoteAsync(ProductNote productNote);

        /// <summary>
        /// Import product price from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportProductPriceFromXlsxAsync(Stream stream);

        /// <summary>
        /// Import product stock from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportProductStockFromXlsxAsync(Stream stream);

        /// <summary>
        /// Deletes a Product attribute value condition
        /// </summary>
        /// <param name="pavCondition">Product attribute value condition</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductAttributeValueConditionAsync(ProductAttributeValueCondition pavCondition);

        /// <summary>
        /// Gets all Product attribute value conditions
        /// </summary>
        /// <param name="paValueId">Product attribute value identifier</param>
        /// <param name="showCrossValue">A value indicating whether to show cross reference attribute value condition</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product attribute value identifier
        /// </returns>
        Task<IList<ProductAttributeValueCondition>> GetAllProductAttributeValueConditionByValueIdAsync(int paValueId, bool showCrossValue = false);

        /// <summary>
        /// Inserts a Product note
        /// </summary>
        /// <param name="pavCondition">Product attribute value condition</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductAttributeValueConditionAsync(ProductAttributeValueCondition pavCondition);

        /// <summary>
        /// A value indicating whether this product attribute should has condition on value
        /// </summary>
        /// <param name="paValueId">Product attribute value identifier</param>
        /// <returns>Result</returns>
        Task<bool> ShouldHasConditionOnAttributeValueAsync(int paValueId);

        /// <summary>
        /// get product average report
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the results
        /// </returns>
        Task<(decimal TotalPrice, decimal TotalProductCost)> GetProductAverageReportLine(
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        /// <summary>
        /// Write PDF catalog stock to the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrintProductsToPdfAsync(Stream stream, IList<Product> products);

        /// <summary>
        /// Gets product attribute values by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product attribute value picture id collection who has picture
        /// </returns>
        Task<IList<int>> GetProductAttributeValuesByProductIdAsync(int productId);
    }
}