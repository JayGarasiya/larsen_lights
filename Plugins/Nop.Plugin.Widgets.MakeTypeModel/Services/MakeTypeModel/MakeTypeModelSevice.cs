using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel
{
    /// <summary>
    /// Make type model service
    /// </summary>
    public partial class MakeTypeModelService : IMakeTypeModelService
    {
        #region Fields

        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<MakeProduct> _makeProductRepository;
        protected readonly IRepository<TypeProduct> _typeProductRepository;
        protected readonly IRepository<ModelProduct> _modelProductRepository;
        protected readonly IRepository<ModelProductMapping> _modelProductMappingRepository;
        protected readonly IRepository<ModelCategory> _modelCategoryRepository;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IWorkContext _workContext;
        protected readonly IAclService _aclService;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly ILanguageService _languageService;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        protected readonly IRepository<ProductTag> _productTagRepository;
        protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        protected readonly IProductService _productService;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly ISearchPluginManager _searchPluginManager;
        protected readonly IStoreContext _storeContext;
        protected readonly IWidgetPluginManager _widgetPluginManager;

        #endregion

        #region Ctor

        public MakeTypeModelService(IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<MakeProduct> makeProductRepository,
            IRepository<TypeProduct> typeProductRepository,
            IRepository<ModelProduct> modelProductRepository,
            IRepository<ModelProductMapping> modelProductMappingRepository,
            IRepository<ModelCategory> modelCategoryRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IAclService aclService,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            ILanguageService languageService,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IProductService productService,
            MakeTypeModelSettings makeTypeModelSettings,
            ISearchPluginManager searchPluginManager,
            IStoreContext storeContext,
            ICustomerService customerService,
            IWidgetPluginManager widgetPluginManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _makeProductRepository = makeProductRepository;
            _typeProductRepository = typeProductRepository;
            _modelProductRepository = modelProductRepository;
            _modelProductMappingRepository = modelProductMappingRepository;
            _modelCategoryRepository = modelCategoryRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _aclService = aclService;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _languageService = languageService;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productTagRepository = productTagRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productService = productService;
            _makeTypeModelSettings = makeTypeModelSettings;
            _searchPluginManager = searchPluginManager;
            _widgetPluginManager = widgetPluginManager;
        }

        #endregion

        #region Methods

        #region General

        /// <summary>
        /// Check whether the plugin is active for the current user and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<bool> PluginActiveAsync()
        {
            return await _widgetPluginManager.IsPluginActiveAsync(MakeTypeModelDefaults.SystemName, await _workContext.GetCurrentCustomerAsync());
        }
        #endregion

        #region Make Product

        /// <summary>
        /// Delete a MakeProduct
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteMakeProductAsync(MakeProduct makeProduct)
        {
            ArgumentNullException.ThrowIfNull(makeProduct);

            await _makeProductRepository.DeleteAsync(makeProduct);
        }

        /// <summary>
        /// Gets all MakeProducts
        /// </summary>
        /// <param name="makeName">Make name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProducts
        /// </returns>
        public virtual async Task<IPagedList<MakeProduct>> GetAllMakeProductsAsync(string makeName, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var makeProducts = await _makeProductRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(makeName))
                    query = query.Where(c => c.Name.Contains(makeName));

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return new PagedList<MakeProduct>(makeProducts, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all MakeProducts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProducts
        /// </returns>
        public virtual async Task<IList<MakeProduct>> GetAllMakeProductsAsync()
        {
            var makeProducts = await _makeProductRepository.GetAllAsync(query =>
            {
                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return await makeProducts.ToListAsync();
        }

        /// <summary>
        /// Gets a makeProduct
        /// </summary>
        /// <param name="makeProductId">MakeProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the makeProduct
        /// </returns>
        public virtual async Task<MakeProduct> GetMakeProductByIdAsync(int makeProductId)
        {
            return await _makeProductRepository.GetByIdAsync(makeProductId, cache => default);
        }

        /// <summary>
        /// Inserts a makeProduct
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertMakeProductAsync(MakeProduct makeProduct)
        {
            await _makeProductRepository.InsertAsync(makeProduct);
        }

        /// <summary>
        /// Updates a makeProduct
        /// </summary>
        /// <param name="makeProduct">MakeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateMakeProductAsync(MakeProduct makeProduct)
        {
            //update makeProduct
            await _makeProductRepository.UpdateAsync(makeProduct);
        }

        /// <summary>
        /// Gets a MakeProduct
        /// </summary>
        /// <param name="makeName">Make name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the MakeProduct
        /// </returns>
        public virtual async Task<MakeProduct> GetMakeProductByNameAsync(string makeName)
        {
            var query = from mp in _makeProductRepository.Table
                        orderby mp.Name
                        where mp.Name == makeName
                        select mp;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Type Product

        /// <summary>
        /// Deletes a TypeProduct
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteTypeProductAsync(TypeProduct typeProduct)
        {
            ArgumentNullException.ThrowIfNull(typeProduct);

            await _typeProductRepository.DeleteAsync(typeProduct);
        }

        /// <summary>
        /// Gets all TypeProducts
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProducts
        /// </returns>
        public virtual async Task<IPagedList<TypeProduct>> GetAllTypeProductsAsync(string typeName, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var typeProducts = await _typeProductRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                    query = query.Where(c => c.Name.Contains(typeName));

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return new PagedList<TypeProduct>(typeProducts, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all TypeProducts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProducts
        /// </returns>
        public virtual async Task<IList<TypeProduct>> GetAllTypeProductsAsync()
        {
            var typeProducts = await _typeProductRepository.GetAllAsync(query =>
            {
                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return await typeProducts.ToListAsync();
        }

        /// <summary>
        /// Gets a TypeProduct
        /// </summary>
        /// <param name="typeProductId">TypeProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        public virtual async Task<TypeProduct> GetTypeProductByIdAsync(int typeProductId)
        {
            return await _typeProductRepository.GetByIdAsync(typeProductId, cache => default);
        }

        /// <summary>
        /// Inserts a TypeProduct
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertTypeProductAsync(TypeProduct typeProduct)
        {
            await _typeProductRepository.InsertAsync(typeProduct);
        }

        /// <summary>
        /// Updates a TypeProduct
        /// </summary>
        /// <param name="typeProduct">TypeProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateTypeProductAsync(TypeProduct typeProduct)
        {
            //update typeProduct
            await _typeProductRepository.UpdateAsync(typeProduct);
        }

        /// <summary>
        /// Gets a TypeProduct
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        public virtual async Task<TypeProduct> GetTypeProductByNameAsync(string typeName)
        {
            var query = from mp in _typeProductRepository.Table
                        orderby mp.Name
                        where mp.Name == typeName
                        select mp;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Model Product

        /// <summary>
        /// Deletes a ModelProduct
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteModelProductAsync(ModelProduct modelProduct)
        {
            ArgumentNullException.ThrowIfNull(modelProduct);

            await _modelProductRepository.DeleteAsync(modelProduct);
        }

        /// <summary>
        /// Gets all ModelProducts
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
        public virtual async Task<IPagedList<ModelProduct>> GetAllModelProductsAsync(string modelName, bool searchInDescription = false, string makeName = null, string typeName = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _modelProductRepository.Table;

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                query = query.Where(c => c.Name.Contains(modelName));

                if (searchInDescription)
                    query = query.Where(c => c.Description.Contains(modelName));
            }

            if (!string.IsNullOrWhiteSpace(makeName))
                query = query.Where(c => c.MakeName.Equals(makeName));

            if (!string.IsNullOrWhiteSpace(typeName))
                query = query.Where(c => c.TypeName.Equals(typeName));

            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all ModelProducts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProducts
        /// </returns>
        public virtual async Task<IList<ModelProduct>> GetAllModelProductsAsync(string makeName = null, string typeName = null)
        {
            var modelProducts = await _modelProductRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(makeName))
                    query = query.Where(c => c.MakeName.Equals(makeName));

                if (!string.IsNullOrWhiteSpace(typeName))
                    query = query.Where(c => c.TypeName.Equals(typeName));

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return await modelProducts.ToListAsync();
        }

        /// <summary>
        /// Gets a ModelProduct By Product Identifier
        /// </summary>
        /// <param name="modelProductId">ModelProduct identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProduct
        /// </returns>
        public virtual async Task<ModelProduct> GetModelProductByIdAsync(int modelProductId)
        {
            return await _modelProductRepository.GetByIdAsync(modelProductId, cache => default);
        }

        /// <summary>
        /// Inserts a ModelProduct
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertModelProductAsync(ModelProduct modelProduct)
        {
            await _modelProductRepository.InsertAsync(modelProduct);
        }

        /// <summary>
        /// Updates a ModelProduct
        /// </summary>
        /// <param name="modelProduct">ModelProduct</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateModelProductAsync(ModelProduct modelProduct)
        {
            //update modelProduct
            await _modelProductRepository.UpdateAsync(modelProduct);
        }

        /// <summary>
        /// Gets all ModelProducts By Product Identifier
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProducts
        /// </returns>
        public virtual async Task<IList<ModelProduct>> GetAllModelProductsByProductIdAsync(int productId)
        {
            if (productId == 0)
                return new List<ModelProduct>();

            var product = await _productService.GetProductByIdAsync(productId);
            var associatedProducts = new List<int>();
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //initial product
                associatedProducts.Add(productId);

                //associated products
                associatedProducts.AddRange((await _productService.GetAssociatedProductsAsync(product.Id)).Select(ap => ap.Id).ToList());
            }

            return await _modelProductRepository.GetAllAsync(query =>
            {
                var modelProductMappingQuery = _modelProductMappingRepository.Table;

                if (!associatedProducts.Any())
                    query = query.Where(pc => modelProductMappingQuery.Any(c => c.ModelId == pc.Id && c.ProductId == productId));
                else
                    query = query.Where(pc => modelProductMappingQuery.Any(c => c.ModelId == pc.Id && associatedProducts.Any(ap => c.ProductId == ap)));

                return query
                    .OrderBy(pc => pc.DisplayOrder)
                    .ThenBy(pc => pc.Name);
            });
        }

        /// <summary>
        /// Gets a ModelProduct
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="description">Model description</param>
        /// <param name="makeName">Make name</param>
        /// <param name="typeName">Type name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the TypeProduct
        /// </returns>
        public virtual async Task<ModelProduct> GetModelProductAsync(string name, string description, string makeName, string typeName)
        {
            var query = from mp in _modelProductRepository.Table
                        orderby mp.Name
                        where mp.Name == name && mp.Description == description && mp.MakeName == makeName && mp.TypeName == typeName
                        select mp;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Model Product Mapping

        /// <summary>
        /// Deletes a Model Product Mapping
        /// </summary>
        /// <param name="modelProductMapping">ModelProductMapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteModelProductMappingAsync(ModelProductMapping modelProductMapping)
        {
            ArgumentNullException.ThrowIfNull(modelProductMapping);

            await _modelProductMappingRepository.DeleteAsync(modelProductMapping);
        }

        /// <summary>
        /// Gets all Model Product Mappings
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
        public virtual async Task<IPagedList<ModelProductMapping>> GetAllModelProductMappingsAsync(int modelId = 0, int productId = 0,
            string modelName = null, bool searchInDescription = false, string makeName = null, string typeName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _modelProductMappingRepository.Table
                    .Join(_modelProductRepository.Table, x => x.ModelId, y => y.Id, (x, y) => new { modelProductMapping = x, modelProduct = y })
                    .Where(z => z.modelProduct.Id == z.modelProductMapping.ModelId)
                    .Select(z => new { z.modelProduct, z.modelProductMapping });

            if (modelId > 0)
                query = query.Where(c => c.modelProductMapping.ModelId == modelId);

            if (productId > 0)
                query = query.Where(c => c.modelProductMapping.ProductId == productId);

            if (!string.IsNullOrEmpty(modelName) || !string.IsNullOrEmpty(makeName) || !string.IsNullOrEmpty(typeName))
            {
                if (!string.IsNullOrWhiteSpace(modelName))
                {
                    query = query.Where(c => c.modelProduct.Name.Contains(modelName));

                    if (searchInDescription)
                        query = query.Where(c => c.modelProduct.Description.Contains(modelName));
                }

                if (!string.IsNullOrWhiteSpace(makeName))
                    query = query.Where(c => c.modelProduct.MakeName.Equals(makeName));

                if (!string.IsNullOrWhiteSpace(typeName))
                    query = query.Where(c => c.modelProduct.TypeName.Equals(typeName));

            }

            query = query.OrderBy(c => c.modelProduct.DisplayOrder).ThenBy(c => c.modelProduct.Name);

            var result = query.Select(c => c.modelProductMapping).Distinct();

            //database layer paging
            return await result.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all Model Product Mappings
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        public virtual async Task<IList<ModelProductMapping>> GetAllModelProductMappingsAsync(int modelId = 0, int productId = 0)
        {
            var modelProducts = await _modelProductMappingRepository.GetAllAsync(query =>
            {
                if (modelId > 0)
                    query = query.Where(c => c.ModelId == modelId);

                if (productId > 0)
                    query = query.Where(c => c.ProductId == productId);

                return query.OrderBy(c => c.ProductId).ThenBy(c => c.Id);
            });

            return await modelProducts.ToListAsync();
        }

        /// <summary>
        /// Gets all Model Product Mappings By Model Identifier
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        public virtual async Task<IList<ModelProductMapping>> GetAllModelProductMappingsByModelIdAsync(int modelId = 0)
        {
            var modelProducts = await _modelProductMappingRepository.GetAllAsync(query =>
            {
                if (modelId > 0)
                    query = query.Where(c => c.ModelId == modelId);

                return query.OrderBy(c => c.ProductId).ThenBy(c => c.Id);
            });

            return await modelProducts.ToListAsync();
        }

        /// <summary>
        /// Gets All Model Product Mapping By Model and Product Identifier
        /// </summary>
        /// <param name="modelId">Model identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMappings
        /// </returns>
        public virtual async Task<ModelProductMapping> GetAllModelProductMappingsByModelIdAndProductIdAsync(int modelId, int productId)
        {
            var query = from mp in _modelProductMappingRepository.Table
                        orderby mp.ProductId, mp.Id
                        where mp.ModelId == modelId && mp.ProductId == productId
                        select mp;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a ModelProductMapping
        /// </summary>
        /// <param name="modelProductMappingId">ModelProductMapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelProductMapping
        /// </returns>
        public virtual async Task<ModelProductMapping> GetModelProductMappingByIdAsync(int modelProductMappingId)
        {
            return await _modelProductMappingRepository.GetByIdAsync(modelProductMappingId, cache => default);
        }

        /// <summary>
        /// Inserts a ModelProductMapping
        /// </summary>
        /// <param name="modelProductMapping">ModelProductMapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertModelProductMappingAsync(ModelProductMapping modelProductMapping)
        {
            await _modelProductMappingRepository.InsertAsync(modelProductMapping);
        }

        #endregion

        #region Model Category

        /// <summary>
        /// Gets all ModelCategories
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategories
        /// </returns>
        public virtual async Task<IPagedList<ModelCategory>> GetAllModelCategoriesAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _modelCategoryRepository.Table;

            if (!string.IsNullOrEmpty(name))
                query = (from mc in query
                         join c in _categoryRepository.Table on mc.CategoryId equals c.Id
                         where c.Name.Contains(name)
                         select mc);

            var modelCategories = query.OrderBy(x => x.DisplayOrder);

            return await modelCategories.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all ModelCategories
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategories
        /// </returns>
        public virtual async Task<IList<ModelCategory>> GetAllModelCategoriesAsync(bool showHidden = false)
        {
            var modelCategories = await _modelCategoryRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(c => c.Published);

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            return await modelCategories.ToListAsync();
        }

        /// <summary>
        /// Gets a ModelCategory
        /// </summary>
        /// <param name="modelCategoryId">ModelCategory identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ModelCategory
        /// </returns>
        public virtual async Task<ModelCategory> GetModelCategoryByIdAsync(int modelCategoryId)
        {
            return await _modelCategoryRepository.GetByIdAsync(modelCategoryId, cache => default);
        }

        /// <summary>
        /// Inserts a ModelCategory
        /// </summary>
        /// <param name="modelCategory">ModelCategory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertModelCategoryAsync(ModelCategory modelCategory)
        {
            await _modelCategoryRepository.InsertAsync(modelCategory);
        }

        /// <summary>
        /// Updates a ModelCategory
        /// </summary>
        /// <param name="modelCategory">ModelCategory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateModelCategoryAsync(ModelCategory modelCategory)
        {
            //update modelCategory
            await _modelCategoryRepository.UpdateAsync(modelCategory);
        }

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
        public virtual async Task<IPagedList<Product>> SearchProductsAsync(
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
            string model = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            //apply ACL constraints
            if (!showHidden)
            {
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

                if (activeSearchProvider is not null)
                {
                    productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
                }
                else
                {
                    productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords)) ||
                            searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords ||
                            searchSku && p.Sku == keywords
                        select p.Id;

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                            from lp in _localizedPropertyRepository.Table
                            let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkShortDesc = searchDescriptions &&
                                            lp.LocaleKey == nameof(Product.ShortDescription) &&
                                            lp.LocaleValue.Contains(keywords)
                            where
                                lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                            select lp.EntityId);
                    }
                }

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            pc.First().DisplayOrder
                        };

                    if (_makeTypeModelSettings.KitProductCategoryId > 0)
                    {
                        var hasModelCategory = categoryIds.Contains(_makeTypeModelSettings.KitProductModelCategoryId);

                        productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        where !hasModelCategory ? !hasModelCategory :
                                _productCategoryRepository.Table.Any(pam => pam.CategoryId == _makeTypeModelSettings.KitProductCategoryId && pam.ProductId == p.Id)
                        orderby pc.DisplayOrder, p.Name
                        select p;
                    }
                    else
                    {
                        productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                    }
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            if (!string.IsNullOrEmpty(make) || !string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(model))
            {
                var productModelQuery = _modelProductMappingRepository.Table
                    .Join(_modelProductRepository.Table, x => x.ModelId, y => y.Id, (x, y) => new { modelProductMapping = x, modelProduct = y })
                    .Join(_productRepository.Table, x => x.modelProductMapping.ProductId, z => z.Id, (x, z) => new { x.modelProductMapping, x.modelProduct, product = z })
                    .Where(z => z.modelProduct.Id == z.modelProductMapping.ModelId)
                    .Select(z => new { z.modelProduct, z.modelProductMapping, z.product });

                if (!string.IsNullOrEmpty(make))
                    productModelQuery = productModelQuery.Where(x => x.modelProduct.MakeName.Equals(make));

                if (!string.IsNullOrEmpty(type))
                    productModelQuery = productModelQuery.Where(x => x.modelProduct.TypeName.Equals(type));

                if (!string.IsNullOrEmpty(model))
                    productModelQuery = productModelQuery.Where(x => x.modelProduct.Name.Equals(model));

                var productModels = productModelQuery.Select(x => new ModelProductMapping
                {
                    Id = x.modelProductMapping.Id,
                    ModelId = x.modelProductMapping.ModelId,
                    ProductId = !x.product.VisibleIndividually && x.product.ParentGroupedProductId > 0 ? x.product.ParentGroupedProductId : x.modelProductMapping.ProductId
                }).Distinct();

                productsQuery = productsQuery
                .Join(productModels, x => x.Id, y => y.ProductId, (x, y) => new { Product = x, ModelProductMapping = y })
                .Where(z => z.Product.Id == z.ModelProductMapping.ProductId)
                .Select(z => z.Product)
                .Distinct();
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Get products identifiers by the specified keywords
        /// </summary>
        /// <param name="keywords">Keywords</param>
        /// <param name="isLocalized">A value indicating whether to search in localized properties</param>
        /// <returns>The task result contains product identifiers</returns>
        public async Task<List<int>> SearchProductsAsync(string keywords, bool isLocalized)
        {
            var searchDescriptions = true;
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            var productsByKeywords =
                        (from p in _productRepository.Table
                         where p.Name.Contains(keywords) ||
                             searchDescriptions &&
                                 (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords)) ||
                             p.ManufacturerPartNumber == keywords ||
                             p.Sku == keywords
                         select p.Id)
                        .Union(from p in _productRepository.Table
                               where p.Name.Contains(keywords) ||
                                   searchDescriptions &&
                                       (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords)) ||
                                   p.ManufacturerPartNumber == keywords ||
                                   p.Sku == keywords && p.ParentGroupedProductId > 0 && !p.VisibleIndividually
                               select p.ParentGroupedProductId);

            if (isLocalized)
            {
                productsByKeywords = productsByKeywords.Union(
                    from lp in _localizedPropertyRepository.Table
                    let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                    lp.LocaleValue.Contains(keywords)
                    let checkShortDesc = searchDescriptions &&
                                    lp.LocaleKey == nameof(Product.ShortDescription) &&
                                    lp.LocaleValue.Contains(keywords)
                    where
                            lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                    select lp.EntityId);
            }

            return productsByKeywords.ToList();
        }

        #endregion

        #endregion
    }
}
