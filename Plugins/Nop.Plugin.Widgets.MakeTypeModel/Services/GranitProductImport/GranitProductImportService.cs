using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Globalization;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport
{
    /// <summary>
    /// Granit product import service
    /// </summary>
    public class GranitProductImportService : IGranitProductImportService
    {
        #region Fields

        protected readonly IRepository<Domain.GranitImport.GranitProductImport> _granitProductImportRepository;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        protected readonly IRepository<GranitDataImport> _granitDataImportRepository;
        protected readonly IRepository<GranitAttrImport> _granitAttrImportRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IProductService _productService;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly INopFileProvider _fileProvider;
        protected readonly ICategoryService _categoryService;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IPictureService _pictureService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly ISpecificationAttributeService _specificationAttributeService;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IDiscountService _discountService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;
        protected readonly IAclService _aclService;
        protected readonly ICustomerService _customerService;
        protected readonly IProductTagService _productTagService;

        #endregion

        #region Ctor

        public GranitProductImportService(IRepository<Domain.GranitImport.GranitProductImport> granitProductImportRepository,
            IProductService productService,
            CatalogSettings catalogSettings,
            INopFileProvider fileProvider,
            ICategoryService categoryService,
            MakeTypeModelSettings makeTypeModelSettings,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IHtmlFormatter htmlFormatter,
            ISpecificationAttributeService specificationAttributeService,
            IUrlRecordService urlRecordService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IAclService aclService,
            ICustomerService customerService,
            IProductTagService productTagService,
            IRepository<GranitDataImport> granitDataImportRepository,
            IRepository<GranitAttrImport> granitAttrImportRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Category> categoryRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<Product> productRepository)
        {
            _granitProductImportRepository = granitProductImportRepository;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _fileProvider = fileProvider;
            _categoryService = categoryService;
            _makeTypeModelSettings = makeTypeModelSettings;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _htmlFormatter = htmlFormatter;
            _specificationAttributeService = specificationAttributeService;
            _urlRecordService = urlRecordService;
            _discountService = discountService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _aclService = aclService;
            _customerService = customerService;
            _productTagService = productTagService;
            _granitDataImportRepository = granitDataImportRepository;
            _granitAttrImportRepository = granitAttrImportRepository;
            _manufacturerRepository = manufacturerRepository;
            _categoryRepository = categoryRepository;
            _specificationAttributeRepository = specificationAttributeRepository;
            _productRepository = productRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get Mime Type From File Path
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }

        /// <summary>
        /// Update local values
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="model">CategoryModel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(Category category, CategoryModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(category, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(category, seName, localized.LanguageId);
            }
        }

        /// <summary>
        /// Save store mappings for the passed entity
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="model">CategoryModel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        protected virtual async Task SaveStoreMappingsAsync(Category category, CategoryModel model)
        {
            category.LimitedToStores = model.SelectedStoreIds.Any();
            await _categoryService.UpdateCategoryAsync(category);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(category);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(category, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        /// <summary>
        /// Save category acl 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="model">CategoryModel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        protected virtual async Task SaveCategoryAclAsync(Category category, CategoryModel model)
        {
            category.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _categoryService.UpdateCategoryAsync(category);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(category);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(category, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        /// <summary>
        /// Find or create category  
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="image">image</param>
        /// <param name="productId">product identifier</param>
        /// <param name="parentCategoryId">parent category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        private async Task<Category> FindOrCreateCategoryAsync(string name, string image, int productId, int? parentCategoryId)
        {
            // 1. Try to find an existing category with this name under the same parent
            var existingCategory = await _categoryService
                .GetAllCategoriesByParentCategoryIdAsync(parentCategoryId ?? 0, showHidden: true)
                .ContinueWith(t => t.Result.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

            var isNew = existingCategory == null;
            existingCategory = existingCategory ?? new Category();

            // Get the full path to the image folder
            var imageFolderPath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/image/"));

            // Find the file that matches the image name with any extension (e.g., .jpg, .png, etc.)
            var allImageFiles = Directory.GetFiles(imageFolderPath);
            var matchedFile = allImageFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(image, StringComparison.OrdinalIgnoreCase));

            var model = new CategoryModel()
            {
                Name = name,
                CategoryTemplateId = 1,
                ParentCategoryId = parentCategoryId ?? 0,
                PageSize = _catalogSettings.DefaultCategoryPageSize,
                PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions,
                Published = true,
                AllowCustomersToSelectPageSize = true,
                PriceRangeFiltering = true,
                ManuallyPriceRange = true,
                PriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom,
                PriceTo = NopCatalogDefaults.DefaultPriceRangeTo,
            };

            existingCategory.CreatedOnUtc = DateTime.UtcNow;
            existingCategory.UpdatedOnUtc = DateTime.UtcNow;

            if (isNew)
            {
                existingCategory = model.ToEntity<Category>();

                await _categoryService.InsertCategoryAsync(existingCategory);
                // After insertion, the category object will have its Id populated by EF Core (if tracked), but just to be safe:
                existingCategory = (await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId ?? 0)).Where(x => x.Name == name).FirstOrDefault();

                // add image only if category is new
                if (matchedFile != null && existingCategory != null)
                {
                    var newSeoName = await _pictureService.GetPictureSeNameAsync(Path.GetFileNameWithoutExtension(matchedFile));
                    var mimeType = GetMimeTypeFromFilePath(matchedFile);

                    if (!string.IsNullOrEmpty(mimeType))
                    {
                        var newPictureBinary = await _fileProvider.ReadAllBytesAsync(matchedFile);
                        var newPicture = await _pictureService.InsertPictureAsync(newPictureBinary, mimeType, seoFilename: await _pictureService.GetPictureSeNameAsync(matchedFile));
                        if (newPicture != null)
                        {
                            await _pictureService.SetSeoFilenameAsync(newPicture.Id, newSeoName);
                            existingCategory.PictureId = newPicture.Id;
                        }
                    }
                }
            }
            else
            {
                await _categoryService.UpdateCategoryAsync(existingCategory);
            }

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(existingCategory, model.SeName, existingCategory.Name, true);
            await _urlRecordService.SaveSlugAsync(existingCategory, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(existingCategory, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = existingCategory.Id });
            }

            //ACL (customer roles)
            await SaveCategoryAclAsync(existingCategory, model);

            //stores
            await SaveStoreMappingsAsync(existingCategory, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), existingCategory.Name), existingCategory);

            await _categoryService.UpdateCategoryAsync(existingCategory);

            return existingCategory;
        }

        /// <summary>
        /// Find or create category  
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="specName">specName</param>
        /// <param name="specValue">specValue</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        private async Task AddOrUpdateSpecificationAttributeAsync(Product product, string specName, string specValue)
        {
            if (string.IsNullOrWhiteSpace(specName?.Trim()) || string.IsNullOrWhiteSpace(specValue?.Trim()))
                return;

            var specAttribute = (await _specificationAttributeService.GetAllSpecificationAttributesAsync())
                .Where(x => x.Name.Trim().Equals(specName.Trim(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            // Create attribute if it doesn't exist
            if (specAttribute == null)
            {
                specAttribute = new SpecificationAttribute { Name = specName };
                await _specificationAttributeService.InsertSpecificationAttributeAsync(specAttribute);
            }

            var specAttributeOptions = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specAttribute.Id);

            var specOption = specAttributeOptions
                .FirstOrDefault(o => o.Name.Equals(specValue, StringComparison.InvariantCultureIgnoreCase));

            // Create option if it doesn't exist
            if (specOption == null)
            {
                specOption = new SpecificationAttributeOption
                {
                    Name = specValue,
                    SpecificationAttributeId = specAttribute.Id
                };
                await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specOption);
            }

            // Check if already mapped to product
            var existingMapping = (await _specificationAttributeService
                .GetProductSpecificationAttributesAsync(product.Id))
                .FirstOrDefault(x => x.AttributeTypeId == (int)SpecificationAttributeType.Option
                                     && x.SpecificationAttributeOptionId == specOption.Id);

            var isNew = existingMapping == null;
            var mapping = isNew ? new ProductSpecificationAttribute() : existingMapping;

            mapping.ProductId = product.Id;
            mapping.SpecificationAttributeOptionId = specOption.Id;
            mapping.AttributeTypeId = (int)SpecificationAttributeType.Option;
            mapping.CustomValue = null;
            mapping.AllowFiltering = false;
            mapping.ShowOnProductPage = true;
            mapping.DisplayOrder = 0;

            if (isNew)
                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(mapping);
            else
                await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(mapping);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granit Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteGranitProductImportAsync(Domain.GranitImport.GranitProductImport graniteProductImport)
        {
            ArgumentNullException.ThrowIfNull(graniteProductImport);

            try
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/granitproductimport/" + graniteProductImport.FileName));

                if (_fileProvider.FileExists(filePath))
                    _fileProvider.DeleteFile(filePath);
            }
            catch
            {
                // ignored
            }

            await _granitProductImportRepository.DeleteAsync(graniteProductImport);
        }

        /// <summary>
        /// Gets all temp data records
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the temp data records.
        /// </returns>
        public virtual async Task<IList<GranitDataImport>> GetAllTempDataRecordsAsync()
        {
            return await _granitDataImportRepository.GetAllAsync(query =>
            {
                return query;
            });
        }

        /// <summary>
        /// Delete data records
        /// </summary>
        /// <param name="dataRecords">Temp1</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteDataRecordsFromTempAsync(IList<GranitDataImport> dataRecords)
        {
            await _granitDataImportRepository.DeleteAsync(dataRecords);
        }

        /// <summary>
        /// Gets all temp attr records
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the temp attr records.
        /// </returns>
        public virtual async Task<IList<GranitAttrImport>> GetAllGranitAttrImportRecordsAsync()
        {
            return await _granitAttrImportRepository.GetAllAsync(query =>
            {
                return query;
            });
        }

        /// <summary>
        /// Delete attr records
        /// </summary>
        /// <param name="dataRecords">Temp2</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAttrRecordsFromTempAsync(IList<GranitAttrImport> attrRecords)
        {
            await _granitAttrImportRepository.DeleteAsync(attrRecords);
        }

        /// <summary>
        /// Delete all granit product import complete files
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAllGranitProductImportCompleteFilesAsync()
        {
            var productImports = await GetAllGranitProductImportAsync(importStatusIds: new List<int> { (int)GranitProductImportStatusEnum.Complete });
            foreach (var productImport in productImports)
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/graniteproductimport/" + productImport.FileName));
                if (!_fileProvider.FileExists(filePath))
                    continue;

                try
                {
                    _fileProvider.DeleteFile(filePath);
                }
                catch
                {
                    // ignored
                }
            }

            //remove mast entries
            productImports.ToList().ForEach(async p => await DeleteGranitProductImportAsync(p));
        }

        /// <summary>
        /// Gets a granit product import by product import identifier
        /// </summary>
        /// <param name="graniteProductImportId">GraniteProduct import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import
        /// </returns>
        public virtual async Task<Domain.GranitImport.GranitProductImport> GetGranitProductImportByIdAsync(int productImportId)
        {
            return await _granitProductImportRepository.GetByIdAsync(productImportId, cache => default);
        }

        /// <summary>
        /// Inserts a granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granite Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertGranitProductImportAsync(Domain.GranitImport.GranitProductImport productImport)
        {
            ArgumentNullException.ThrowIfNull(productImport);

            await _granitProductImportRepository.InsertAsync(productImport);
        }

        /// <summary>
        /// Updates the granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granite Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateGranitProductImportAsync(Domain.GranitImport.GranitProductImport productImport)
        {
            ArgumentNullException.ThrowIfNull(productImport);

            await _granitProductImportRepository.UpdateAsync(productImport);
        }

        /// <summary>
        /// Gets a value indicating whether is import file already exists
        /// </summary>
        /// <returns>The task result contains a value indicating whether is import file already exists</returns>
        public virtual bool IsImportFileExists()
        {
            return _granitProductImportRepository.Table.Any();
        }

        /// <summary>
        /// Gets all granit product imports
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="importTypeIds">Import type identifiers; null to load all product imports</param>
        /// <param name="importStatusIds">Import status identifiers; null to load all product imports</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product imports
        /// </returns>
        public virtual async Task<IPagedList<Domain.GranitImport.GranitProductImport>> GetAllGranitProductImportAsync(string fileName = null,
           List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _granitProductImportRepository.Table;

            if (!string.IsNullOrEmpty(fileName))
                query = query.Where(o => o.FileName.Contains(fileName));

            if (importStatusIds != null && importStatusIds.Any())
                query = query.Where(o => importStatusIds.Contains(o.ImportStatusId));

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a granit product import
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual Task<Domain.GranitImport.GranitProductImport> GetGranitProductImportAsync()
        {
            var query = _granitProductImportRepository.Table;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a list of data records from temp table
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<IList<GranitDataImport>> GetDataRecordsFromTempAsync(int startRow = 0, int endRow = 0)
        {
            return await _granitDataImportRepository.Table
                .OrderBy(t => t.Id)
                .Skip(startRow)
                .Take(endRow - startRow + 1)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a list of attr records from temp table
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public virtual async Task<IList<GranitAttrImport>> GetAttrRecordsBySKUAsync(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from p in _granitAttrImportRepository.Table
                        where p.ProductSKU == sku
                        select p;

            var records = await query.ToListAsync();

            return records;
        }

        /// <summary>
        /// Gets amanufacturer by name
        /// </summary>
        /// <param name="manufacturerName">Manufacturer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer
        /// </returns>
        public virtual async Task<Manufacturer> GetManufacturerByNameAsync(string manufacturerName)
        {
            if (string.IsNullOrEmpty(manufacturerName))
                return null;

            manufacturerName = manufacturerName.Trim();

            var query = from m in _manufacturerRepository.Table
                        where m.Name == manufacturerName
                        select m;

            var records = await query.FirstOrDefaultAsync();

            return records;
        }

        /// <summary>
        /// Gets category by name
        /// </summary>
        /// <param name="categoryName">Category</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        public virtual async Task<Category> GetCategoryByNameAsync(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return null;

            categoryName = categoryName.Trim();

            var query = from c in _categoryRepository.Table
                        where c.Name == categoryName
                        select c;

            return await _categoryRepository.Table.Where(c => c.Name == categoryName).OrderBy(c => c.Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets specification attribute by name
        /// </summary>
        /// <param name="specificationAttributeName">SpecificationAttribute</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        public virtual async Task<SpecificationAttribute> GetSpecificationAttributeByNameAsync(string specificationAttributeName)
        {
            if (string.IsNullOrEmpty(specificationAttributeName))
                return null;

            specificationAttributeName = specificationAttributeName.Trim();

            var query = from c in _specificationAttributeRepository.Table
                        where c.Name == specificationAttributeName
                        select c;

            var records = await query.FirstOrDefaultAsync();

            return records;
        }

        /// <summary>
        /// Import granit products from XLSX file
        /// </summary>
        /// <param name="graniteProductImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        public virtual async Task ImportGranitProductsAsync(IList<GranitDataImport> dataRecords, Domain.GranitImport.GranitProductImport graintproductImport, int firstDataRecord, int maxDataRecord)
        {
            //Get the full path to the image folder
            var imageFolderPath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/image/"));
            var firstDataRow = firstDataRecord;

            foreach (var record in dataRecords)
            {
                var parentCategory = _makeTypeModelSettings.GranitCategoryId;

                var productSKU = record.ProductSKU;

                if (string.IsNullOrEmpty(productSKU))
                {
                    firstDataRow++;
                    continue;
                }

                var product = await _productService.GetProductBySkuAsync(productSKU);
                var isNew = product == null;
                product = product ?? new Product();

                var productName = record.ProductName;
                product.Name = productName;
                product.Sku = productSKU;
                product.ManufacturerPartNumber = productSKU;

                var productFullDesc = record.FullDescription;
                if (!string.IsNullOrEmpty(productFullDesc))
                {
                    var description = _htmlFormatter.StripTags(_htmlFormatter.ConvertHtmlToPlainText(productFullDesc, decode: true));
                    product.FullDescription = description;
                }

                // sets product weight in lbs
                var productWeight = record.Weight;
                if (!string.IsNullOrEmpty(productWeight))
                {
                    if (double.TryParse(productWeight,
                        NumberStyles.Any,
                        new CultureInfo("de-DE"),
                        out double weightInKg))
                    {
                        double weightInLbs = weightInKg * 2.20462;

                        // Assuming product.Weight is of type decimal or double
                        product.Weight = Math.Round((decimal)weightInLbs, 4); // round to 4 decimal places
                    }
                }

                if (isNew)
                {
                    //set some default values if not specified
                    product.OrderMinimumQuantity = 1;
                    product.OrderMaximumQuantity = 10000;
                    product.ProductType = ProductType.SimpleProduct;
                    product.VisibleIndividually = true;
                    product.AllowCustomerReviews = true;
                    product.IsShipEnabled = true;
                    product.Published = false;
                    product.CreatedOnUtc = DateTime.UtcNow;

                    //set product stock
                    product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                    product.StockQuantity = 10000;
                }

                //sets the vendor for the product
                if (_makeTypeModelSettings.GranitVendorId > 0)
                    product.VendorId = _makeTypeModelSettings.GranitVendorId;

                //sets the warehouse for the product
                if (_makeTypeModelSettings.GranitWarehouseId > 0)
                    product.WarehouseId = _makeTypeModelSettings.GranitWarehouseId;

                var packOfQuantity = record.PackOfQuantity;
                
                product.OrderMinimumQuantity = 1;

                if (isNew)
                    await _productService.InsertProductAsync(product);
                else
                    await _productService.UpdateProductAsync(product);

                //sets product tags
                var tags = record.ProductTags ?? string.Empty;
                if (!string.IsNullOrEmpty(tags))
                {
                    var productTags = tags.Split(',').Select(i => i.Trim()).ToArray();

                    await _productTagService.UpdateProductTagsAsync(product, productTags);
                }
                
                //search engine name
                var seName = isNew ? string.Empty : await _urlRecordService.GetSeNameAsync(product, 0);
                await _urlRecordService.SaveSlugAsync(product, await _urlRecordService.ValidateSeNameAsync(product, seName, product.Name, true), 0);

                //sets the product category
                var parentId = 0;
                if (parentCategory > 0)
                {
                    var subCategories = new List<(string name, string image)>();

                    for (int i = 1; i <= 5; i++)
                    {
                        var nameProp = typeof(GranitDataImport).GetProperty($"Sub{i}CategoryName");
                        var imageProp = typeof(GranitDataImport).GetProperty($"Sub{i}CategoryImage") ?? typeof(GranitDataImport).GetProperty($"Sub{i}CategoryPicture");

                        var nameValue = nameProp?.GetValue(record) as string;
                        var imageValue = imageProp?.GetValue(record) as string;

                        if (!string.IsNullOrWhiteSpace(nameValue))
                            subCategories.Add((nameValue.Trim(), imageValue?.Trim()));
                    }

                    // build category tree
                    parentId = parentCategory;
                    foreach (var (name, image) in subCategories)
                    {
                        // Create/find category in DB with parentId, name & image
                        var category = await FindOrCreateCategoryAsync(name, image, product.Id, parentId);

                        // Update parentId for next subcategory
                        parentId = category.Id;
                    }

                    // You now have the final categoryId to assign to product
                    var finalCategoryId = parentId;
                    var existingProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

                    if (product != null)
                    {
                        var productCategoryMapping = new ProductCategory();
                        if (finalCategoryId > 0)
                        {
                            if (_categoryService.FindProductCategory(existingProductCategories, product.Id, finalCategoryId) == null)
                            {
                                productCategoryMapping.CategoryId = finalCategoryId;
                                productCategoryMapping.ProductId = product.Id;

                                var proCategoryDisplayOrder = record.CategoryDisplayOrder;

                                if (!string.IsNullOrEmpty(proCategoryDisplayOrder))
                                    productCategoryMapping.DisplayOrder = Convert.ToInt32(proCategoryDisplayOrder);

                                await _categoryService.InsertProductCategoryAsync(productCategoryMapping);
                            }
                        }
                    }
                }

                //sets the manufacturer for the product
                var productManufacturer = record.Manufacturer;
                if (!string.IsNullOrEmpty(productManufacturer))
                {
                    var manufacturer = await GetManufacturerByNameAsync(productManufacturer);

                    var isManufacturerNew = manufacturer == null;

                    manufacturer ??= new Manufacturer();

                    if (isManufacturerNew)
                    {
                        manufacturer.CreatedOnUtc = DateTime.UtcNow;
                        manufacturer.Name = productManufacturer;

                        //default values
                        manufacturer.PageSize = _catalogSettings.DefaultManufacturerPageSize;
                        manufacturer.PageSizeOptions = _catalogSettings.DefaultManufacturerPageSizeOptions;
                        manufacturer.Published = true;
                        manufacturer.AllowCustomersToSelectPageSize = true;

                        await _manufacturerService.InsertManufacturerAsync(manufacturer);
                    }
                    else
                    {
                        manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                        await _manufacturerService.UpdateManufacturerAsync(manufacturer);
                    }

                    var existingProductmanufacturers = await _manufacturerService
                    .GetProductManufacturersByManufacturerIdAsync(manufacturer.Id, showHidden: true);

                    //whether product manufacturer with such parameters already exists
                    var existingMapping = _manufacturerService.FindProductManufacturer(existingProductmanufacturers, product.Id, manufacturer.Id);
                    if (existingMapping == null)
                    {
                        //insert the new product manufacturer mapping
                        await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                        {
                            ManufacturerId = manufacturer.Id,
                            ProductId = product.Id,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        });
                    }
                }

                //mapps the related products to the product
                var relatedProductsRaw = record.RelatedProducts;
                if (!string.IsNullOrWhiteSpace(relatedProductsRaw))
                {
                    var relatedProductSkus = relatedProductsRaw.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    foreach (var productSku in relatedProductSkus)
                    {
                        var relatedProduct = await _productService.GetProductBySkuAsync(productSku);
                        if (relatedProduct != null)
                        {
                            var existingRelatedProducts = await _productService.GetRelatedProductsByProductId1Async(product.Id, showHidden: true);
                            if (_productService.FindRelatedProduct(existingRelatedProducts, product.Id, relatedProduct.Id) == null)
                            {
                                await _productService.InsertRelatedProductAsync(new RelatedProduct
                                {
                                    ProductId1 = product.Id,
                                    ProductId2 = relatedProduct.Id,
                                    DisplayOrder = 1
                                });
                            }
                        }
                        else
                        {
                            var newRelatedProduct = new Product();
                            newRelatedProduct.Name = productSku;
                            newRelatedProduct.Sku = productSku;
                            newRelatedProduct.VendorId = _makeTypeModelSettings.GranitVendorId;
                            newRelatedProduct.Published = false;

                            await _productService.InsertProductAsync(newRelatedProduct);

                            await _productService.InsertRelatedProductAsync(new RelatedProduct
                            {
                                ProductId1 = product.Id,
                                ProductId2 = newRelatedProduct.Id,
                                DisplayOrder = 1
                            });
                        }
                    }
                }

                if (_makeTypeModelSettings.UpdateProductImage)
                {
                    //sets the product image
                    var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                    if (productPictures.Count > 0)
                    {
                        foreach (var productPicture in productPictures)
                        {
                            var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                            if (picture != null)
                            {
                                await _pictureService.DeletePictureAsync(picture);
                            }
                            await _productService.DeleteProductPictureAsync(productPicture);
                        }
                    }

                    var mainImage = record.ProductImage;
                    if (!string.IsNullOrEmpty(mainImage))
                    {
                        // Find the file that matches the image name with any extension (e.g., .jpg, .png, etc.)
                        var allImageFiles = Directory.GetFiles(imageFolderPath);
                        var matchedFile = allImageFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(mainImage, StringComparison.OrdinalIgnoreCase));

                        if (matchedFile != null && product != null)
                        {
                            var newSeoName = await _pictureService.GetPictureSeNameAsync(Path.GetFileNameWithoutExtension(matchedFile));
                            var mimeType = GetMimeTypeFromFilePath(matchedFile);
                            if (!string.IsNullOrEmpty(mimeType))
                            {
                                var newPictureBinary = await _fileProvider.ReadAllBytesAsync(matchedFile);
                                var newPicture = await _pictureService.InsertPictureAsync(newPictureBinary, mimeType, seoFilename: await _pictureService.GetPictureSeNameAsync(matchedFile));
                                if (newPicture != null)
                                {
                                    await _pictureService.SetSeoFilenameAsync(newPicture.Id, newSeoName);
                                    await _productService.InsertProductPictureAsync(new ProductPicture
                                    {
                                        PictureId = newPicture.Id,
                                        ProductId = product.Id,
                                        DisplayOrder = 0
                                    });
                                }
                            }
                        }
                    }

                    var allImages = record.ProductImages;

                    if (!string.IsNullOrEmpty(allImages))
                    {
                        var productPics = await _pictureService.GetPicturesByProductIdAsync(product.Id);

                        var allImageFiles = Directory.GetFiles(imageFolderPath);
                        var imageNames = allImages.Split(',').Select(i => i.Trim()).ToList();

                        foreach (var imageName in imageNames)
                        {
                            var imageNameWithoutExt = Path.GetFileNameWithoutExtension(imageName);
                            var matchedFile = allImageFiles
                                .FirstOrDefault(f => Path.GetFileName(f).Equals(imageName, StringComparison.OrdinalIgnoreCase));

                            if (matchedFile != null && product != null)
                            {
                                var newSeoName = await _pictureService.GetPictureSeNameAsync(Path.GetFileNameWithoutExtension(matchedFile));
                                var alreadyExists = false;

                                foreach (var productPic in productPics)
                                {
                                    if (productPic.SeoFilename.Equals(imageNameWithoutExt, StringComparison.OrdinalIgnoreCase))
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                if (!alreadyExists)
                                {
                                    var mimeType = GetMimeTypeFromFilePath(matchedFile);
                                    if (!string.IsNullOrEmpty(mimeType))
                                    {
                                        var pictureBinary = await _fileProvider.ReadAllBytesAsync(matchedFile);
                                        var newPicture = await _pictureService.InsertPictureAsync(pictureBinary, mimeType, await _pictureService.GetPictureSeNameAsync(imageName));

                                        if (newPicture != null)
                                        {
                                            await _pictureService.SetSeoFilenameAsync(newPicture.Id, seoFilename: newSeoName);
                                            await _productService.InsertProductPictureAsync(new ProductPicture
                                            {
                                                PictureId = newPicture.Id,
                                                ProductId = product.Id,
                                                DisplayOrder = 1
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var attrDataRecords = (await GetAttrRecordsBySKUAsync(productSKU)).ToList();

                if (attrDataRecords.Count > 0)
                {
                    // Get relevant rows
                    var technicalRows = attrDataRecords
                        .Where(r => r.SpecificationName.Trim().Equals("Technical data", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                    var descriptionRows = attrDataRecords
                        .Where(r => r.SpecificationName.Trim().Equals("Description", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var technicalText = string.Join(" ", technicalRows.Select(r => r.SpecificationValue?.Trim()));
                    var descriptionText = string.Join(" ", descriptionRows.Select(r => r.SpecificationValue?.Trim()));

                    var hasTech = !string.IsNullOrEmpty(technicalText);
                    var hasDesc = !string.IsNullOrEmpty(descriptionText);

                    if (hasTech && hasDesc)
                    {
                        var formattedTechnicalText = technicalText.TrimEnd();
                        if (!formattedTechnicalText.EndsWith("."))
                        {
                            formattedTechnicalText += ".";
                        }

                        product.ShortDescription = $"{formattedTechnicalText} Pack of {packOfQuantity}";

                        var specificationAttrName = descriptionRows.First().SpecificationName?.Trim();
                        var specificationAttrValue = descriptionRows.First().SpecificationValue?.Trim();

                        await AddOrUpdateSpecificationAttributeAsync(product, specificationAttrName, specificationAttrValue);
                    }
                    else if (hasTech)
                    {
                        var formattedTechnicalText = technicalText.TrimEnd();
                        if (!formattedTechnicalText.EndsWith("."))
                        {
                            formattedTechnicalText += ".";
                        }

                        product.ShortDescription = $"{formattedTechnicalText} Pack of {packOfQuantity}";
                    }
                    else if (hasDesc)
                    {
                        var formattedDescriptionText = descriptionText.TrimEnd();
                        if (!formattedDescriptionText.EndsWith("."))
                        {
                            formattedDescriptionText += ".";
                        }

                        product.ShortDescription = $"{formattedDescriptionText} Pack of {packOfQuantity}";
                    }
                    else
                    {
                        product.ShortDescription = $"Pack of {packOfQuantity}";
                    }

                    // Add other types as specification attributes
                    var specRows = attrDataRecords
                                   .Where(r =>
                                   {
                                       var name = r.SpecificationName?.Trim();
                                       return !string.IsNullOrWhiteSpace(name)
                                           && !name.Equals("Technical data", StringComparison.OrdinalIgnoreCase)
                                           && !name.Equals("Description", StringComparison.OrdinalIgnoreCase);
                                   }).ToList();

                    foreach (var specRow in specRows)
                    {
                        var specName = specRow.SpecificationName?.Trim();
                        var specValue = specRow.SpecificationValue?.Trim();

                        await AddOrUpdateSpecificationAttributeAsync(product, specName, specValue);
                    }

                    await _productService.UpdateProductAsync(product);
                }

                //update row processed 
                firstDataRow++;
                graintproductImport.DataRowNumber = firstDataRow;
                graintproductImport.ImportStatus = GranitProductImportStatusEnum.Processing;
                await UpdateGranitProductImportAsync(graintproductImport);

                if (firstDataRow >= maxDataRecord)
                    break;
            }
        }

        /// <summary>
        /// Gets a products by SKU array
        /// </summary>
        /// <param name="skuArray">SKU array</param>
        /// <param name="vendorId">Vendor ID; 0 to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public async Task<IList<Product>> GetProductsBySkuAsync(string[] skuArray)
        {
            ArgumentNullException.ThrowIfNull(skuArray);

            var products = await _productRepository.Table.Where(p => !p.Deleted).ToListAsync();

            return products
            .Where(p => skuArray.Contains(p.Sku, StringComparer.Ordinal))
            .ToList();
        }
        #endregion

    }
}
