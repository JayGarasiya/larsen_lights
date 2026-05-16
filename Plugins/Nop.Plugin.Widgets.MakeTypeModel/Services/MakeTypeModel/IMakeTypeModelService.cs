using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel
{
    /// <summary>
    /// Make type model service 
    /// </summary>
    public partial interface IMakeTypeModelService
    {
        #region General

        /// <summary>
        /// Checks whether the 'Previously Purchased' plugin is active for the current customer and store.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result indicates whether the plugin is active.
        /// </returns>
        Task<bool> PluginActiveAsync();

        #endregion

        #region Make Product

        /// <summary>
        /// Delete a make product
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteMakeProductAsync(MakeProduct makeProduct);

        /// <summary>
        /// Gets all make products
        /// </summary>
        /// <param name="makeName">Make name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProducts
        /// </returns>
        Task<IPagedList<MakeProduct>> GetAllMakeProductsAsync(string makeName, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all make products
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProducts
        /// </returns>
        Task<IList<MakeProduct>> GetAllMakeProductsAsync();

        /// <summary>
        /// Gets a make product by make product identifier
        /// </summary>
        /// <param name="makeProductId">MakeProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the makeProduct
        /// </returns>
        Task<MakeProduct> GetMakeProductByIdAsync(int makeProductId);

        /// <summary>
        /// Insert a make product 
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertMakeProductAsync(MakeProduct makeProduct);

        /// <summary>
        /// Update a make product
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateMakeProductAsync(MakeProduct makeProduct);

        /// <summary>
        /// Gets a make product by name
        /// </summary>
        /// <param name="makeName">Make name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProduct
        /// </returns>
        Task<MakeProduct> GetMakeProductByNameAsync(string makeName);

        #endregion

        #region Type Product

        /// <summary>
        /// Delete a type product
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteTypeProductAsync(TypeProduct typeProduct);

        /// <summary>
        /// Gets all type products
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProducts
        /// </returns>
        Task<IPagedList<TypeProduct>> GetAllTypeProductsAsync(string typeName, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all type products
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProducts
        /// </returns>
        Task<IList<TypeProduct>> GetAllTypeProductsAsync();

        /// <summary>
        /// Gets a type product by identifier
        /// </summary>
        /// <param name="typeProductId">TypeProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        Task<TypeProduct> GetTypeProductByIdAsync(int typeProductId);

        /// <summary>
        /// Inserts a type product
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertTypeProductAsync(TypeProduct typeProduct);

        /// <summary>
        /// Updates a type product
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateTypeProductAsync(TypeProduct typeProduct);

        /// <summary>
        /// Gets a type product by name
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        Task<TypeProduct> GetTypeProductByNameAsync(string typeName);

        #endregion

        #region Model Product

        /// <summary>
        /// Deletes a model product
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteModelProductAsync(ModelProduct modelProduct);

        /// <summary>
        /// Gets all model products
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="searchInDescription">Search in model description</param>
        /// <param name="makeName">Make name</param>
        /// <param name="typeName">Type name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProducts
        /// </returns>
        Task<IPagedList<ModelProduct>> GetAllModelProductsAsync(string modelName, bool searchInDescription = false, string makeName = null, string typeName = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all model products
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProducts
        /// </returns>
        Task<IList<ModelProduct>> GetAllModelProductsAsync(string makeName = null, string typeName = null);

        /// <summary>
        /// Gets a model product by identifier
        /// </summary>
        /// <param name="modelProductId">ModelProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProduct
        /// </returns>
        Task<ModelProduct> GetModelProductByIdAsync(int modelProductId);

        /// <summary>
        /// Inserts a model product
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertModelProductAsync(ModelProduct modelProduct);

        /// <summary>
        /// Updates a model product
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateModelProductAsync(ModelProduct modelProduct);

        /// <summary>
        /// Gets all model products by product identifier
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProducts
        /// </returns>
        Task<IList<ModelProduct>> GetAllModelProductsByProductIdAsync(int productId);

        /// <summary>
        /// Gets a model product
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="description">Model description</param>
        /// <param name="makeName">Make name</param>
        /// <param name="typeName">Type name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        Task<ModelProduct> GetModelProductAsync(string name, string description, string makeName, string typeName);

        #endregion

        #region Model Product Mapping

        /// <summary>
        /// Deletes a model product mapping
        /// </summary>
        /// <param name="modelProductMapping">ModelProductMapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteModelProductMappingAsync(ModelProductMapping modelProductMapping);

        /// <summary>
        /// Gets all model product mappings
        /// </summary>
        /// <param name="modelId">Model identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="modelName">Model name</param>
        /// <param name="searchInDescription">Search in model description</param>
        /// <param name="makeName">Make name</param>
        /// <param name="typeName">Type name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        Task<IPagedList<ModelProductMapping>> GetAllModelProductMappingsAsync(int modelId = 0, int productId = 0,
            string modelName = null, bool searchInDescription = false, string makeName = null, string typeName = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all model product mappings
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        Task<IList<ModelProductMapping>> GetAllModelProductMappingsAsync(int modelId = 0, int productId = 0);

        /// <summary>
        /// Gets all model product mappings by model identifier 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        Task<IList<ModelProductMapping>> GetAllModelProductMappingsByModelIdAsync(int modelId = 0);

        /// <summary>
        /// Gets model product mappings by model and product identifier
        /// </summary>
        /// <param name="modelId">Model identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        Task<ModelProductMapping> GetAllModelProductMappingsByModelIdAndProductIdAsync(int modelId, int productId);

        /// <summary>
        /// Gets a model product mapping by identifier
        /// </summary>
        /// <param name="modelProductMappingId">ModelProductMapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMapping
        /// </returns>
        Task<ModelProductMapping> GetModelProductMappingByIdAsync(int modelProductMappingId);

        /// <summary>
        /// Inserts a model product mapping
        /// </summary>
        /// <param name="modelProductMapping">ModelProductMapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertModelProductMappingAsync(ModelProductMapping modelProductMapping);

        #endregion

        #region Model Category

        /// <summary>
        /// Gets all model categories
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategories
        /// </returns>
        Task<IPagedList<ModelCategory>> GetAllModelCategoriesAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all model categories
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategories
        /// </returns>
        Task<IList<ModelCategory>> GetAllModelCategoriesAsync(bool showHidden = false);

        /// <summary>
        /// Gets a model category by model category identifier
        /// </summary>
        /// <param name="modelCategoryId">ModelCategory identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategory
        /// </returns>
        Task<ModelCategory> GetModelCategoryByIdAsync(int modelCategoryId);

        /// <summary>
        /// Inserts a model category
        /// </summary>
        /// <param name="modelCategory">ModelCategory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertModelCategoryAsync(ModelCategory modelCategory);

        /// <summary>
        /// Updates a model category
        /// </summary>
        /// <param name="modelCategory">ModelCategory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateModelCategoryAsync(ModelCategory modelCategory);

        #endregion

        #region Search Products by default with Make, Type, Models

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
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
        /// <param name="make">Make name; null to load all records</param>
        /// <param name="type">Type name; null to load all records</param>
        /// <param name="model">Model name; null to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
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
            bool? overridePublished = null,
            string make = null,
            string type = null,
            string model = null);

        /// <summary>
        /// Get products identifiers by the specified keywords
        /// </summary>
        /// <param name="keywords">Keywords</param>
        /// <param name="isLocalized">A value indicating whether to search in localized properties</param>
        /// <returns>The task result contains product identifiers</returns>
        Task<List<int>> SearchProductsAsync(string keywords, bool isLocalized);

        #endregion

    }
}
