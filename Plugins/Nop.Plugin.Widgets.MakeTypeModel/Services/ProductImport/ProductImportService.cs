using ClosedXML.Excel;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport
{
    /// <summary>
    /// Product import service
    /// </summary>
    public partial class ProductImportService : IProductImportService
    {
        #region Constants

        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA512";

        private const string UPLOADS_TEMP_PATH = "~/App_Data/TempUploads";

        #endregion

        #region Fields

        protected readonly IRepository<Domain.ProductImport.ProductImport> _productImportRepository;
        protected readonly IProductService _productService;
        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly INopFileProvider _fileProvider;
        protected readonly ICategoryService _categoryService;
        protected readonly IStoreContext _storeContext;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly INopDataProvider _dataProvider;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger _logger;
        protected readonly IPictureService _pictureService;
        protected readonly MediaSettings _mediaSettings;
        protected readonly IProductTagService _productTagService;
        protected readonly ISpecificationAttributeService _specificationAttributeService;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IAclService _aclService;
        protected readonly IStoreService _storeService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerService _customerService;
        protected readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public ProductImportService(IRepository<Domain.ProductImport.ProductImport> productImportRepository,
            IProductService productService,
            IMakeTypeModelService makeTypeModelService,
            CatalogSettings catalogSettings,
            INopFileProvider fileProvider,
            ICategoryService categoryService,
            IStoreContext storeContext,
            MakeTypeModelSettings makeTypeModelSettings,
            INopDataProvider dataProvider,
            IUrlRecordService urlRecordService,
            ILocalizationService localizationService,
            IStoreMappingService storeMappingService,
            IManufacturerService manufacturerService,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            IPictureService pictureService,
            MediaSettings mediaSettings,
            IProductTagService productTagService,
            ISpecificationAttributeService specificationAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IAclService aclService,
            IStoreService storeService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService)
        {
            _productImportRepository = productImportRepository;
            _productService = productService;
            _makeTypeModelService = makeTypeModelService;
            _catalogSettings = catalogSettings;
            _fileProvider = fileProvider;
            _categoryService = categoryService;
            _storeContext = storeContext;
            _makeTypeModelSettings = makeTypeModelSettings;
            _dataProvider = dataProvider;
            _urlRecordService = urlRecordService;
            _localizationService = localizationService;
            _storeMappingService = storeMappingService;
            _manufacturerService = manufacturerService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _productTagService = productTagService;
            _specificationAttributeService = specificationAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _aclService = aclService;
            _storeService = storeService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get property list by excel cells
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="worksheet">Excel worksheet</param>
        /// <returns>Property list</returns>
        public static WorkbookMetadata<T> GetWorkbookMetadata<T>(IXLWorkbook workbook, IList<Language> languages)
        {
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var properties = new List<PropertyByName<T>>();
            var localizedProperties = new List<PropertyByName<T>>();
            var localizedWorksheets = new List<IXLWorksheet>();

            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Row(1).Cell(poz);

                    if (string.IsNullOrEmpty(cell?.Value.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            foreach (var ws in workbook.Worksheets.Skip(1))
                if (languages.Any(l => l.UniqueSeoCode.Equals(ws.Name, StringComparison.InvariantCultureIgnoreCase)))
                    localizedWorksheets.Add(ws);

            if (localizedWorksheets.Any())
            {
                // get the first worksheet in the workbook
                var localizedWorksheet = localizedWorksheets.First();

                poz = 1;
                while (true)
                {
                    try
                    {
                        var cell = localizedWorksheet.Row(1).Cell(poz);

                        if (string.IsNullOrEmpty(cell?.Value.ToString()))
                            break;

                        poz += 1;
                        localizedProperties.Add(new PropertyByName<T>(cell.Value.ToString()));
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return new WorkbookMetadata<T>()
            {
                DefaultProperties = properties,
                LocalizedProperties = localizedProperties,
                DefaultWorksheet = worksheet,
                LocalizedWorksheets = localizedWorksheets
            };
        }

        /// <summary>
        /// Make first letter of a string upper case (with maximum performance)
        /// </summary>
        /// <param name="input">string inout</param>
        /// <returns>Make first letter of a string upper case</returns>
        public static string FirstCharToUpper(string input, string splitBy = " ")
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            //Remove the last char of String '/'
            if (input.EndsWith("/"))
                input = input.Remove(input.Length - 1);

            if (input.Contains(" / "))
                input = input.Replace(" / ", "@@@");

            var inputs = input.Trim().Split(splitBy).ToList();
            var output = string.Empty;
            foreach (var text in inputs)
            {
                if (string.IsNullOrEmpty(text))
                    continue;

                switch (text)
                {
                    case var dash when text.Contains("-"):
                        var dashString = FirstCharToUpper(text, "-");
                        output = string.IsNullOrEmpty(output) ? dashString : $"{output} {dashString}";
                        break;
                    case var slashspace when text.Contains("@@@"):
                        var slashspaceString = FirstCharToUpper(text, "@@@");
                        output = string.IsNullOrEmpty(output) ? slashspaceString : $"{output} {slashspaceString}";
                        break;
                    case var slash when text.Contains("/"):
                        var slashString = FirstCharToUpper(text, "/");
                        output = string.IsNullOrEmpty(output) ? slashString : $"{output} {slashString}";
                        break;
                    default:
                        output = string.IsNullOrEmpty(output) ? char.ToUpper(text.First()) + text.Substring(1) :
                        splitBy.Equals("-") ? $"{output}-{char.ToUpper(text.First()) + text.Substring(1)}" :
                        splitBy.Equals("@@@") ? $"{output} / {char.ToUpper(text.First()) + text.Substring(1)}" :
                        splitBy.Equals("/") ? $"{output}/{char.ToUpper(text.First()) + text.Substring(1)}" :
                        $"{output} {char.ToUpper(text.First()) + text.Substring(1)}";
                        break;
                }

            }

            return output;
        }

        /// <summary>
        /// Remove unwanted charset
        /// </summary>
        /// <param name="text">string text</param>
        protected static string RemoveUnwantedCharset(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var m = Regex.Matches(text, @"_x([0-9a-fA-F]{4})_", RegexOptions.IgnoreCase);
            for (var i = m.Count - 1; i >= 0; i--)
            {
                text = text.Remove(m[i].Index, m[i].Length);
            }

            return text;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task LogPictureInsertErrorAsync(string picturePath, Exception ex)
        {
            var extension = _fileProvider.GetFileExtension(picturePath);
            var name = _fileProvider.GetFileNameWithoutExtension(picturePath);

            var point = string.IsNullOrEmpty(extension) ? string.Empty : ".";
            var fileName = _fileProvider.FileExists(picturePath) ? $"{name}{point}{extension}" : string.Empty;

            await _logger.ErrorAsync($"Insert picture failed (file name: {fileName})", ex);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task ImportProductImagesUsingServicesAsync(string[] productPictures, Product product, bool isNew)
        {
            foreach (var picturePath in productPictures)
            {
                if (string.IsNullOrEmpty(picturePath))
                    continue;

                var mimeType = GetMimeTypeFromFilePath(picturePath);
                var newPictureBinary = await _fileProvider.ReadAllBytesAsync(picturePath);
                var pictureAlreadyExists = false;
                if (!isNew)
                {
                    //compare with existing product pictures
                    var existingPictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                    foreach (var existingPicture in existingPictures)
                    {
                        var existingBinary = await _pictureService.LoadPictureBinaryAsync(existingPicture);
                        //picture binary after validation (like in database)
                        var validatedPictureBinary = await _pictureService.ValidatePictureAsync(newPictureBinary, mimeType, null);
                        if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                            !existingBinary.SequenceEqual(newPictureBinary))
                            continue;
                        //the same picture content
                        pictureAlreadyExists = true;
                        break;
                    }
                }

                if (pictureAlreadyExists)
                    continue;

                try
                {
                    var newPicture = await _pictureService.InsertPictureAsync(newPictureBinary, mimeType, await _pictureService.GetPictureSeNameAsync(product.Name));
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = 1,
                        ProductId = product.Id
                    });
                    await _productService.UpdateProductAsync(product);
                }
                catch (Exception ex)
                {
                    await LogPictureInsertErrorAsync(picturePath, ex);
                }
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task ImportProductImagesUsingHashAsync(string[] productPictures, Product product, bool isNew)
        {
            //performance optimization, load all pictures hashes
            //it will only be used if the images are stored in the SQL Server database (not compact)
            var trimByteCount = _dataProvider.SupportedLengthOfBinaryHash - 1;
            var productsImagesIds = await _productService.GetProductsImagesIdsAsync(new int[] { product.Id });

            var allProductPictureIds = productsImagesIds.SelectMany(p => p.Value);
            var allPicturesHashes = allProductPictureIds.Any() ? await _dataProvider.GetFieldHashesAsync<PictureBinary>(p => allProductPictureIds.Contains(p.PictureId),
               p => p.PictureId, p => p.BinaryData) : new Dictionary<int, string>();

            foreach (var picturePath in productPictures)
            {
                if (string.IsNullOrEmpty(picturePath))
                    continue;
                try
                {
                    var mimeType = GetMimeTypeFromFilePath(picturePath);
                    var newPictureBinary = await _fileProvider.ReadAllBytesAsync(picturePath);
                    var pictureAlreadyExists = false;
                    //  var seoFileName = await _pictureService.GetPictureSeNameAsync(picturePath.N);
                    if (!isNew)
                    {
                        var newImageHash = HashHelper.CreateHash(
                            newPictureBinary,
                            IMAGE_HASH_ALGORITHM,
                            trimByteCount);

                        var newValidatedImageHash = HashHelper.CreateHash(
                            await _pictureService.ValidatePictureAsync(newPictureBinary, mimeType, null),
                            IMAGE_HASH_ALGORITHM,
                            trimByteCount);

                        var imagesIds = productsImagesIds.ContainsKey(product.Id)
                            ? productsImagesIds[product.Id]
                            : Array.Empty<int>();

                        pictureAlreadyExists = allPicturesHashes.Where(p => imagesIds.Contains(p.Key))
                            .Select(p => p.Value)
                            .Any(p =>
                                p.Equals(newImageHash, StringComparison.OrdinalIgnoreCase) ||
                                p.Equals(newValidatedImageHash, StringComparison.OrdinalIgnoreCase));
                    }

                    if (pictureAlreadyExists)
                        continue;

                    var newPicture = await _pictureService.InsertPictureAsync(newPictureBinary, mimeType, await _pictureService.GetPictureSeNameAsync(product.Name));

                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = 1,
                        ProductId = product.Id
                    });

                    await _productService.UpdateProductAsync(product);
                }
                catch (Exception ex)
                {
                    await LogPictureInsertErrorAsync(picturePath, ex);
                }
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<string> DownloadFileAsync(string urlString, IList<string> downloadedFiles)
        {
            if (string.IsNullOrEmpty(urlString))
                return string.Empty;

            if (!Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                return urlString;

            //ensure that temp directory is created
            var tempDirectory = _fileProvider.MapPath(UPLOADS_TEMP_PATH);
            _fileProvider.CreateDirectory(tempDirectory);

            var fileName = _fileProvider.GetFileName(urlString);
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var filePath = _fileProvider.Combine(tempDirectory, fileName);
            try
            {
                var client = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                var fileData = await client.GetByteArrayAsync(urlString);
                await using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    fs.Write(fileData, 0, fileData.Length);

                downloadedFiles?.Add(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Download image failed", ex);
            }

            return string.Empty;
        }

        protected virtual string[] ParseValues(string input)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
                return result.ToArray();

            var values = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        /// <summary>
        /// Get parent categories 
        /// </summary>
        /// <param name="currentCategories">List<Category></param>
        /// <param name="importCategory">Category</param>
        /// <param name="processCategory">Category</param>
        protected virtual List<Category> GetParentCategories(List<Category> currentCategories, Category importCategory, Category processCategory)
        {
            var parentCategories = new List<Category>();

            if (importCategory is null)
                return parentCategories;

            if (processCategory is null)
                return parentCategories;

            //get import category 
            if (!parentCategories.Any(c => c.Id == importCategory.Id) && !importCategory.Published)
                parentCategories.Add(importCategory);

            if (importCategory.Id == processCategory.Id)
                return parentCategories;

            var parentCategory = currentCategories.FirstOrDefault(c => c.Id == processCategory.ParentCategoryId);
            if (parentCategory == null)
                return parentCategories;

            if (!parentCategory.Published)
                parentCategories.Add(parentCategory);

            parentCategories.AddRange(GetParentCategories(currentCategories, importCategory, parentCategory));

            return parentCategories;
        }

        /// <summary>
        /// Prepare categories 
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<(List<int> categoryIds, IList<Category> allCategoryList, Dictionary<CategoryKey, Category> allCategories)> PrepareCategoiesAsync(int categoryId, int storeId)
        {
            var categoryIdsTemp = new List<int>();
            IList<Category> allCategoryListTemp = new List<Category>();
            Dictionary<CategoryKey, Category> allCategoriesTemp;

            categoryIdsTemp = new List<int>() { categoryId };

            //include subcategories
            categoryIdsTemp.AddRange(
                await _categoryService.GetChildCategoryIdsAsync(categoryId, storeId, true));
            if (categoryIdsTemp.Contains(0))
                categoryIdsTemp.Remove(0);

            allCategoryListTemp = await _categoryService.GetCategoriesByIdsAsync(categoryIdsTemp.ToArray());

            //performance optimization, load all categories in one SQL request
            var allCategoryList = await _categoryService.GetAllCategoriesAsync(showHidden: true);

            try
            {
                allCategoriesTemp = await allCategoryListTemp.ToDictionaryAwaitAsync(async c =>
                {
                    var keyName =
                        await _categoryService.GetFormattedBreadCrumbAsync(c, allCategoryListTemp);

                     return new CategoryKey(keyName, c);
                },
                c => new ValueTask<Category>(c));
            }
            catch (ArgumentException)
            {
                //categories with the same name are not supported in the same category level
                throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.CategoriesWithSameNameNotSupported"));
            }

            return (categoryIdsTemp, allCategoryListTemp, allCategoriesTemp);
        }

        /// <summary>
        /// Update locales value 
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
        /// Update picture seo names 
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdatePictureSeoNamesAsync(Category category)
        {
            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(category.Name));
        }

        /// <summary>
        /// Save category acl 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="model">CategoryModel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
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
        /// Save store mapping 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="model">CategoryModel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
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
        /// Create category
        /// </summary>
        /// <param name="categoryName">categoryName</param>
        /// <param name="parentCategoryId">parent category identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<int> CreateCategoryAsync(string categoryName, int parentCategoryId)
        {
            var model = new CategoryModel()
            {
                Name = categoryName,
                CategoryTemplateId = 1,
                ParentCategoryId = parentCategoryId,
                PageSize = _catalogSettings.DefaultCategoryPageSize,
                PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions,
                Published = true,
                AllowCustomersToSelectPageSize = true,
                PriceRangeFiltering = true,
                ManuallyPriceRange = true,
                PriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom,
                PriceTo = NopCatalogDefaults.DefaultPriceRangeTo,
            };

            var category = model.ToEntity<Category>();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.InsertCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
            }

            await _categoryService.UpdateCategoryAsync(category);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //ACL (customer roles)
            await SaveCategoryAclAsync(category, model);

            //stores
            await SaveStoreMappingsAsync(category, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), category.Name), category);

            return category.Id;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductImportAsync(Domain.ProductImport.ProductImport productImport)
        {
            ArgumentNullException.ThrowIfNull(productImport);

            try
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/productimport/" + productImport.FileName));

                if (_fileProvider.FileExists(filePath))
                    _fileProvider.DeleteFile(filePath);
            }
            catch
            {
                // ignored
            }

            await _productImportRepository.DeleteAsync(productImport);
        }

        /// <summary>
        /// Delete all product import complete files
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAllProductImportCompleteFilesAsync()
        {
            var productImports = await GetAllProductImportAsync(importStatusIds: new List<int> { (int)ProductImportStatusEnum.Complete });
            foreach (var productImport in productImports)
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/productimport/" + productImport.FileName));
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
            productImports.ToList().ForEach(async p => await DeleteProductImportAsync(p));
        }

        /// <summary>
        /// Gets a product import by product import identifier
        /// </summary>
        /// <param name="productImportId">Product import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import
        /// </returns>
        public virtual async Task<Domain.ProductImport.ProductImport> GetProductImportByIdAsync(int productImportId)
        {
            return await _productImportRepository.GetByIdAsync(productImportId, cache => default);
        }

        /// <summary>
        /// Inserts a product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductImportAsync(Domain.ProductImport.ProductImport productImport)
        {
            ArgumentNullException.ThrowIfNull(productImport);

            await _productImportRepository.InsertAsync(productImport);
        }

        /// <summary>
        /// Updates the product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductImportAsync(Domain.ProductImport.ProductImport productImport)
        {
            ArgumentNullException.ThrowIfNull(productImport);

            await _productImportRepository.UpdateAsync(productImport);
        }

        /// <summary>
        /// Gets a value indicating whether is import file name already exists
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>The task result contains a value indicating whether is import file name already exists</returns>
        public virtual bool IsImportFileNameAlreadyExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            return _productImportRepository.Table
                .Where(p => p.FileName == fileName)
                .Any();
        }

        /// <summary>
        /// Gets all product imports
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
        public virtual async Task<IPagedList<Domain.ProductImport.ProductImport>> GetAllProductImportAsync(string fileName = null,
            List<int> importTypeIds = null, List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productImportRepository.Table;

            if (!string.IsNullOrEmpty(fileName))
                query = query.Where(o => o.FileName.Contains(fileName));

            if (importTypeIds != null && importTypeIds.Any())
                query = query.Where(o => importTypeIds.Contains(o.ImportTypeId));

            if (importStatusIds != null && importStatusIds.Any())
                query = query.Where(o => importStatusIds.Contains(o.ImportStatusId));

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a list of product imports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<IList<Domain.ProductImport.ProductImport>> GetProductImportByStatusAsync()
        {
            var importStatusIds = new List<int> { (int)ProductImportStatusEnum.Processing, (int)ProductImportStatusEnum.Pending };
            var query = _productImportRepository.Table;

            query = query.Where(p => importStatusIds.Contains(p.ImportStatusId));

            query = query.OrderByDescending(o => o.ImportStatusId).ThenBy(o => o.CreatedOnUtc);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Import model fit products from XLSX file
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportModelFitProductsFromXlsxAsync(Domain.ProductImport.ProductImport productImport)
        {
            var file = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/productimport/" + productImport.FileName));
            using var stream = new FileStream(file, FileMode.OpenOrCreate);
            using var xlPackage = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var properties = GetWorkbookMetadata<ModelProduct>(xlPackage, languages);
            var defaultWorksheet = properties.DefaultWorksheet;
            var defaultProperties = properties.DefaultProperties;
            var localizedProperties = properties.LocalizedProperties;

            var manager = new PropertyManager<ModelProduct>(defaultProperties, _catalogSettings, localizedProperties, languages);
            var iRow = productImport.RowNumber == 0 ? 2 : productImport.RowNumber;
            var rowCount = defaultWorksheet.LastRowUsed().RowNumber();
            var maxRow = productImport.RowNumber + _makeTypeModelSettings.HyCapacityModelFitsRecords;
            var lastLoadedProduct = new Product();

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                {
                    iRow++;
                    if (iRow >= rowCount)
                    {
                        productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;
                        await UpdateProductImportAsync(productImport);
                        return;
                    }
                    else
                        continue;
                }

                var modelProduct = new ModelProduct();
                var productSku = string.Empty;

                //Read row data from Xlsx
                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
                foreach (var property in manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "ProductSku":
                            productSku = property.StringValue.Trim();
                            break;
                        case "MakeName":
                            modelProduct.MakeName = property.StringValue;
                            break;
                        case "TypeName":
                            modelProduct.TypeName = property.StringValue;
                            break;
                        case "ModelName":
                            modelProduct.Name = property.StringValue;
                            break;
                        case "ModelDescription":
                            modelProduct.Description = string.IsNullOrEmpty(property.StringValue) || property.StringValue.Equals("null") ? null : property.StringValue;
                            break;
                    }
                }

                var product = await _productService.GetProductBySkuAsync(productSku);
                if (product != null && !string.IsNullOrEmpty(modelProduct.Name) && !string.IsNullOrEmpty(modelProduct.MakeName) && !string.IsNullOrEmpty(modelProduct.TypeName))
                {
                    //delete all old records 
                    if (productImport.DeleteAll && (!lastLoadedProduct?.Sku.Equals(productSku, StringComparison.InvariantCultureIgnoreCase) ?? true))
                    {
                        //delete all model product mapped with importing product
                        var currentModels = await _makeTypeModelService.GetAllModelProductMappingsAsync(0, product.Id);
                        await _dataProvider.BulkDeleteEntitiesAsync(currentModels);

                        lastLoadedProduct = product;
                    }

                    //get make by name if not exist then create new once
                    var makeProduct = await _makeTypeModelService.GetMakeProductByNameAsync(modelProduct.MakeName);
                    if (makeProduct == null)
                    {
                        makeProduct = new MakeProduct() { Name = modelProduct.MakeName, Published = true };
                        await _makeTypeModelService.InsertMakeProductAsync(makeProduct);
                    }

                    //get type by name if not exist then create new once
                    var typeProduct = await _makeTypeModelService.GetTypeProductByNameAsync(modelProduct.TypeName);
                    if (typeProduct == null)
                    {
                        typeProduct = new TypeProduct() { Name = modelProduct.TypeName, Published = true };
                        await _makeTypeModelService.InsertTypeProductAsync(typeProduct);
                    }

                    //get model product by name, description, make and type 
                    var existModelProduct = await _makeTypeModelService.GetModelProductAsync(modelProduct.Name, modelProduct.Description, modelProduct.MakeName, modelProduct.TypeName);
                    if (existModelProduct == null)
                    {
                        modelProduct.Published = true;
                        await _makeTypeModelService.InsertModelProductAsync(modelProduct);
                    }
                    else
                    {
                        existModelProduct.Published = true;
                        await _makeTypeModelService.UpdateModelProductAsync(existModelProduct);
                    }

                    //insert model product mapping
                    var modelProductId = existModelProduct == null ? modelProduct.Id : existModelProduct.Id;
                    if (modelProductId > 0)
                    {
                        var mapping = await _makeTypeModelService.GetAllModelProductMappingsByModelIdAndProductIdAsync(modelProductId, product.Id);
                        if (mapping == null)
                        {
                            var map = new ModelProductMapping
                            {
                                ModelId = modelProductId,
                                ProductId = product.Id
                            };
                            await _makeTypeModelService.InsertModelProductMappingAsync(map);
                        }
                    }
                }

                //update row processed 
                iRow++;
                productImport.RowNumber = iRow;
                productImport.ImportStatus = ProductImportStatusEnum.Processing;
                await UpdateProductImportAsync(productImport);

                if (iRow >= maxRow)
                    break;
            }

            if (iRow >= rowCount)
                productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;

            await UpdateProductImportAsync(productImport);
        }

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductsFromXlsxAsync(Domain.ProductImport.ProductImport productImport)
        {
            var file = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/productimport/" + productImport.FileName));
            using var stream = new FileStream(file, FileMode.OpenOrCreate);
            using var xlPackage = new XLWorkbook(stream);
            // get the first worksheet in the workbook

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            //get hy-capacity categories
            var hycapacity = await _categoryService.GetCategoryByIdAsync(_makeTypeModelSettings.HyCapacityCategoryId);
            var categoryIds = new List<int>();
            IList<Category> allCategoryList = new List<Category>();
            var allCategories = new Dictionary<CategoryKey, Category>();

            (categoryIds, allCategoryList, allCategories) = await PrepareCategoiesAsync(_makeTypeModelSettings.HyCapacityCategoryId, currentStore.Id);

            //performance optimization, load all manufacturers in one SQL request
            var allManufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);

            //performance optimization, load all specification attribute in one SQL request
            var allSpacificationAttributes = await _specificationAttributeService.GetAllSpecificationAttributesAsync();

            //the columns
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
            var properties = GetWorkbookMetadata<Product>(xlPackage, languages);
            var defaultWorksheet = properties.DefaultWorksheet;
            var defaultProperties = properties.DefaultProperties;
            var localizedProperties = properties.LocalizedProperties;

            var manager = new PropertyManager<Product>(defaultProperties, _catalogSettings, localizedProperties, languages);
            var iRow = productImport.RowNumber == 0 ? 2 : productImport.RowNumber;
            var rowCount = defaultWorksheet.LastRowUsed().RowNumber();
            var maxRow = productImport.RowNumber + _makeTypeModelSettings.HyCapacityProductsRecords;

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                {
                    iRow++;
                    if (iRow >= rowCount)
                    {
                        productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;
                        await UpdateProductImportAsync(productImport);
                        return;
                    }
                    else
                        continue;
                }

                //Read row data from Xlsx
                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
                var productWeight = manager.GetDefaultProperty("AP_GEWICHT");
                var tempCategory = manager.GetDefaultProperty("Category");
                if (productImport.ImportStatus == ProductImportStatusEnum.Pending && productImport.DeleteAll)
                {
                    if (!string.IsNullOrWhiteSpace(tempCategory.StringValue))
                    {
                        //Unpublish all products mapped with importing category
                        var currentCategory = allCategoryList.Where(c => c.Name.Equals(FirstCharToUpper(tempCategory.StringValue), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        //database doesn't contain the imported category
                        if (currentCategory == null)
                            throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.DatabaseNotContainCategory"), FirstCharToUpper(tempCategory.StringValue)));

                        var currentCategoryChildIds = await _categoryService.GetChildCategoryIdsAsync(currentCategory.Id, currentStore.Id);
                        currentCategoryChildIds.Add(currentCategory.Id);

                        var currentCategoryProducts = (await _productService.SearchProductsAsync(categoryIds: currentCategoryChildIds, storeId: currentStore.Id, showHidden: true)).ToList();
                        currentCategoryProducts.ForEach(async product =>
                        {
                            product.Published = false;
                            await _productService.UpdateProductAsync(product);
                        });

                        //Unpublish all child categories mapped with importing category
                        currentCategoryChildIds.Remove(currentCategory.Id);
                        var currentCategoryChilds = (await _categoryService.GetCategoriesByIdsAsync(currentCategoryChildIds.ToArray())).ToList();
                        currentCategoryChilds.ForEach(async category =>
                        {
                            category.Published = false;
                            await _categoryService.UpdateCategoryAsync(category);
                        });

                        productImport.ImportStatus = ProductImportStatusEnum.Processing;
                        await UpdateProductImportAsync(productImport);
                    }
                }

                var productSku = manager.GetDefaultProperty("Sku");
                if (string.IsNullOrEmpty(productSku.StringValue))
                {
                    iRow++;
                    continue;
                }

                var product = await _productService.GetProductBySkuAsync(productSku.StringValue);

                var isNew = product == null;
                product = product ?? new Product();

                foreach (var property in manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "Name":
                            product.Name = property.StringValue;
                            break;
                        case "Sku":
                            product.Sku = property.StringValue;
                            product.ManufacturerPartNumber = property.StringValue;
                            break;
                        case "Price":
                            product.Price = property.DecimalValue;
                            break;
                        case "ProductNotes":
                            product.ShortDescription = property.StringValue;
                            break;
                    }
                }

                if (isNew)
                {
                    product.CreatedOnUtc = DateTime.UtcNow;

                    //set some default values if not specified
                    product.OrderMinimumQuantity = 1;
                    product.OrderMaximumQuantity = 10000;
                }

                //set some default values if not specified
                product.ProductType = ProductType.SimpleProduct;
                product.VisibleIndividually = true;
                product.AllowCustomerReviews = true;
                product.IsShipEnabled = true;
                product.Published = true;

                //sets the vendor for the product
                product.VendorId = _makeTypeModelSettings.HyCapacityVendorId;

                //sets the warehouse for the product
                product.WarehouseId = _makeTypeModelSettings.HyCapacityWarehouseId;

                //set product stock
                product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                product.StockQuantity = 10000;

                product.UpdatedOnUtc = DateTime.UtcNow;

                if (isNew)
                    await _productService.InsertProductAsync(product);
                else
                    await _productService.UpdateProductAsync(product);

                //search engine name
                var seName = isNew ? string.Empty : await _urlRecordService.GetSeNameAsync(product, 0);
                await _urlRecordService.SaveSlugAsync(product, await _urlRecordService.ValidateSeNameAsync(product, seName, product.Name, true), 0);

                //performance optimization, load all categories IDs for products in one SQL request
                var productCategoryIds = await _categoryService.GetProductCategoryIdsAsync(new int[] { product.Id });

                var tempProperty = manager.GetDefaultProperty("Breadcrumb");
                if (tempProperty != null)
                {
                    var sitemap = RemoveUnwantedCharset(tempProperty.StringValue);
                    sitemap = Regex.Replace(sitemap, @"(\r)", " ");

                    var categoryList = string.Empty;
                    var breadcrumb = sitemap.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var prevCategoryId = 0;
                    foreach (var category in breadcrumb)
                    {
                        var categoryTemp = category;
                        if (FirstCharToUpper(category).Equals("Home", StringComparison.InvariantCultureIgnoreCase))
                            categoryTemp = category.Replace("Home", hycapacity.Name, StringComparison.InvariantCultureIgnoreCase);

                        categoryList = string.IsNullOrEmpty(categoryList) ? FirstCharToUpper(categoryTemp) : $"{categoryList} >> {FirstCharToUpper(categoryTemp)}";

                        var importedCategories = await categoryList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(categoryName => new CategoryKey(categoryName))
                            .SelectAwait(async categoryKey =>
                            {
                                var rez = allCategories.ContainsKey(categoryKey) ? allCategories[categoryKey].Id : allCategories.Values.FirstOrDefault(c => c.Name == categoryKey.Key)?.Id;

                                if (!rez.HasValue)
                                    rez = allCategories.FirstOrDefault(c => c.Key.Key.ToUpper().Equals(categoryKey.Key.ToUpper(), StringComparison.InvariantCultureIgnoreCase)).Value?.Id;

                                if (!rez.HasValue && int.TryParse(categoryKey.Key, out var id))
                                    rez = id;

                                if (!rez.HasValue)
                                    //database doesn't contain the imported category
                                    throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.DatabaseNotContainCategory"), categoryKey.Key));

                                return rez.Value;
                            }).ToListAsync();

                        if (!importedCategories.Any())
                        {
                            var newCategoryId = await CreateCategoryAsync(categoryTemp, prevCategoryId);
                            importedCategories.Add(newCategoryId);
                            prevCategoryId = newCategoryId;

                            //reload all categories to continue import product
                            (categoryIds, allCategoryList, allCategories) = await PrepareCategoiesAsync(_makeTypeModelSettings.HyCapacityCategoryId, currentStore.Id);
                        }
                        else
                            prevCategoryId = importedCategories.FirstOrDefault();
                    }


                    if (categoryList.ToLower().Contains(FirstCharToUpper(tempCategory.StringValue).ToLower()))
                    {
                        //category mappings
                        var categories = isNew || !productCategoryIds.ContainsKey(product.Id) ? Array.Empty<int>() : productCategoryIds[product.Id];

                        var importedCategories = await categoryList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(categoryName => new CategoryKey(categoryName))
                            .SelectAwait(async categoryKey =>
                            {
                                var rez = allCategories.ContainsKey(categoryKey) ? allCategories[categoryKey].Id : allCategories.Values.FirstOrDefault(c => c.Name == categoryKey.Key)?.Id;

                                if (!rez.HasValue)
                                    rez = allCategories.FirstOrDefault(c => c.Key.Key.ToUpper().Equals(categoryKey.Key.ToUpper(), StringComparison.InvariantCultureIgnoreCase)).Value?.Id;

                                if (!rez.HasValue && int.TryParse(categoryKey.Key, out var id))
                                    rez = id;

                                if (!rez.HasValue)
                                    //database doesn't contain the imported category
                                    throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.DatabaseNotContainCategory"), categoryKey.Key));

                                return rez.Value;
                            }).ToListAsync();

                        if (importedCategories.Any())
                        {
                            foreach (var categoryId in importedCategories)
                            {
                                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                                if (category != null)
                                {
                                    category.Published = true;
                                    await _categoryService.UpdateCategoryAsync(category);

                                    var currentCategory = allCategoryList.Where(c => c.Name.Equals(FirstCharToUpper(tempCategory.StringValue), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                    var parentCategories = GetParentCategories(allCategoryList.ToList(), currentCategory, category);
                                    foreach (var parentCategory in parentCategories)
                                    {
                                        parentCategory.Published = true;
                                        await _categoryService.UpdateCategoryAsync(parentCategory);
                                    }
                                }

                                if (categories.Any(c => c == categoryId))
                                    continue;

                                var productCategory = new ProductCategory
                                {
                                    ProductId = product.Id,
                                    CategoryId = categoryId,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                };
                                await _categoryService.InsertProductCategoryAsync(productCategory);
                            }

                            //delete product categories
                            var deletedProductCategories = await categories.Where(categoryId => !importedCategories.Contains(categoryId))
                                .SelectAwait(async categoryId => (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true)).FirstOrDefault(pc => pc.CategoryId == categoryId)).Where(pc => pc != null).ToListAsync();

                            foreach (var deletedProductCategory in deletedProductCategories)
                                await _categoryService.DeleteProductCategoryAsync(deletedProductCategory);
                        }
                    }
                }

                //performance optimization, load all manufacturers IDs for products in one SQL request
                var allProductsManufacturerIds = await _manufacturerService.GetProductManufacturerIdsAsync(new int[] { product.Id });

                if (_makeTypeModelSettings.HyCapacityManufacturerId > 0)
                {
                    var manufacturerList = (await _manufacturerService.GetManufacturerByIdAsync(_makeTypeModelSettings.HyCapacityManufacturerId))?.Name ?? string.Empty;

                    //manufacturer mappings
                    var manufacturers = isNew || !allProductsManufacturerIds.ContainsKey(product.Id) ? Array.Empty<int>() : allProductsManufacturerIds[product.Id];
                    var importedManufacturers = manufacturerList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => allManufacturers.FirstOrDefault(m => m.Name == x.Trim())?.Id ?? int.Parse(x.Trim())).ToList();
                    foreach (var manufacturerId in importedManufacturers)
                    {
                        if (manufacturers.Any(c => c == manufacturerId))
                            continue;

                        var productManufacturer = new ProductManufacturer
                        {
                            ProductId = product.Id,
                            ManufacturerId = manufacturerId,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
                    }

                    //delete product manufacturers
                    var deletedProductsManufacturers = await manufacturers.Where(manufacturerId => !importedManufacturers.Contains(manufacturerId))
                        .SelectAwait(async manufacturerId => (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).First(pc => pc.ManufacturerId == manufacturerId)).ToListAsync();
                    foreach (var deletedProductManufacturer in deletedProductsManufacturers)
                        await _manufacturerService.DeleteProductManufacturerAsync(deletedProductManufacturer);
                }

                //product tags
                tempProperty = manager.GetDefaultProperty("Reference");
                if (tempProperty != null)
                {
                    defaultWorksheet.Row(iRow).Cell(tempProperty?.PropertyOrderPosition ?? -1).Style.Alignment.WrapText = false;
                    var reference = RemoveUnwantedCharset(tempProperty.StringValue);
                    reference = Regex.Replace(reference, @"(\r|\n)", " ");
                    await _productTagService.UpdateProductTagsAsync(product, ParseValues(reference));
                }

                //product to import images
                var downloadedFiles = new List<string>();
                tempProperty = manager.GetDefaultProperty("Images");
                if (tempProperty != null)
                {
                    var imageList = tempProperty.StringValue;
                    imageList = Regex.Replace(imageList, @"(\r|\n)", " ");
                    var images = ParseValues(imageList).ToList();
                    foreach (var image in images)
                        await DownloadFileAsync(image.Trim(), downloadedFiles);

                    if (_mediaSettings.ImportProductImagesUsingHash && await _pictureService.IsStoreInDbAsync())
                        await ImportProductImagesUsingHashAsync(downloadedFiles.ToArray(), product, isNew);
                    else
                        await ImportProductImagesUsingServicesAsync(downloadedFiles.ToArray(), product, isNew);

                    foreach (var downloadedFile in downloadedFiles)
                    {
                        if (!_fileProvider.FileExists(downloadedFile))
                            continue;

                        try
                        {
                            _fileProvider.DeleteFile(downloadedFile);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                //product specification attribute
                var productSpecificationAttributes = new List<ProductSpecificationAttributeMetadata>();
                if (productImport.DeleteAll)
                {
                    var productSpecAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);
                    if (productSpecAttributes.Any())
                        await _dataProvider.BulkDeleteEntitiesAsync(productSpecAttributes);
                }
                else
                {
                    productSpecificationAttributes = await (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id))
                    .SelectAwait(async attribute =>
                    {
                        var specAttributeOption = await _specificationAttributeService
                        .GetSpecificationAttributeOptionByIdAsync(attribute.SpecificationAttributeOptionId);
                        var specAttribute = await _specificationAttributeService
                            .GetSpecificationAttributeByIdAsync(specAttributeOption.SpecificationAttributeId);

                        return new ProductSpecificationAttributeMetadata()
                        {
                            SpecificationAttributeId = specAttribute.Id,
                            SpecificationAttribute = specAttribute.Name,
                            SpecificationAttributeOptionId = attribute.SpecificationAttributeOptionId,
                            SpecificationAttributeOption = specAttributeOption.Name
                        };
                    }).ToListAsync();
                }

                while (true)
                {
                    var allColumnsAreEmptys = manager.GetDefaultProperties
                    .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmptys)
                        break;

                    //Read row data from Xlsx
                    manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);

                    productSku = manager.GetDefaultProperty("Sku");
                    if (product.Sku.Equals(productSku.StringValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var specName = manager.GetDefaultProperty("Product_Spec_Name");
                        var specValue = manager.GetDefaultProperty("Product_Spec_Value");
                        var specNameString = specName.StringValue.Trim();
                        var specValueString = specValue.StringValue.Trim();

                        if (!string.IsNullOrEmpty(specNameString) && !string.IsNullOrEmpty(specValueString))
                        {
                            var productSpecAttr = productSpecificationAttributes
                                .Where(p => p.SpecificationAttribute.Equals(specNameString, StringComparison.InvariantCultureIgnoreCase) &&
                                p.SpecificationAttributeOption.Equals(specValueString, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

                            if (productSpecAttr != null)
                            {
                                var productSpecificationAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, productSpecAttr.SpecificationAttributeOptionId)).FirstOrDefault();
                                isNew = productSpecificationAttribute == null;
                                if (isNew)
                                    productSpecificationAttribute = new ProductSpecificationAttribute();

                                productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                                productSpecificationAttribute.SpecificationAttributeOptionId = productSpecAttr.SpecificationAttributeOptionId;
                                productSpecificationAttribute.ProductId = product.Id;
                                productSpecificationAttribute.CustomValue = null;
                                productSpecificationAttribute.AllowFiltering = false;
                                productSpecificationAttribute.ShowOnProductPage = true;
                                productSpecificationAttribute.DisplayOrder = 0;

                                if (isNew)
                                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecificationAttribute);
                                else
                                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
                            }
                            else
                            {
                                //if specification attribute option isn't set, try to get first of possible specification attribute option for current specification attribute
                                var specificationAttributeId = 0;
                                var specificationAttribute = allSpacificationAttributes.Where(x => x.Name.Equals(specNameString, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                ;
                                if (specificationAttribute == null)
                                {
                                    specificationAttribute = new SpecificationAttribute() { Name = specNameString };
                                    await _specificationAttributeService.InsertSpecificationAttributeAsync(specificationAttribute);
                                    specificationAttributeId = specificationAttribute.Id;
                                }
                                else
                                    specificationAttributeId = specificationAttribute.Id;

                                var specificationAttributeOptionId = 0;
                                var specificationAttributeOption = (await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttributeId))
                                    .Where(x => x.Name.Equals(specValueString, StringComparison.InvariantCultureIgnoreCase))
                                    .FirstOrDefault();

                                if (specificationAttributeOption == null)
                                {
                                    specificationAttributeOption = new SpecificationAttributeOption() { Name = specValueString, SpecificationAttributeId = specificationAttributeId };
                                    await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specificationAttributeOption);
                                    specificationAttributeOptionId = specificationAttributeOption.Id;
                                }
                                else
                                    specificationAttributeOptionId = specificationAttributeOption.Id;

                                var productSpecificationAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, specificationAttributeOptionId)).FirstOrDefault();
                                isNew = productSpecificationAttribute == null;
                                if (isNew)
                                    productSpecificationAttribute = new ProductSpecificationAttribute();

                                productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                                productSpecificationAttribute.SpecificationAttributeOptionId = specificationAttributeOptionId;
                                productSpecificationAttribute.ProductId = product.Id;
                                productSpecificationAttribute.CustomValue = null;
                                productSpecificationAttribute.AllowFiltering = false;
                                productSpecificationAttribute.ShowOnProductPage = true;
                                productSpecificationAttribute.DisplayOrder = 0;

                                if (isNew)
                                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecificationAttribute);
                                else
                                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
                            }
                        }

                        iRow++;
                    }
                    else
                    {
                        iRow--;
                        break;
                    }
                }

                //update row processed 
                iRow++;
                productImport.RowNumber = iRow;
                productImport.ImportStatus = ProductImportStatusEnum.Processing;
                await UpdateProductImportAsync(productImport);

                if (iRow >= maxRow)
                    break;
            }

            if (iRow >= rowCount)
                productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;

            await UpdateProductImportAsync(productImport);
        }

        /// <summary>
        /// Import products stock from XLSX file
        /// </summary>
        /// <param name="productImport">Product stock import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductStockFromXlsxAsync(Domain.ProductImport.ProductImport productImport)
        {
            var file = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/productimport/" + productImport.FileName));
            using var stream = new FileStream(file, FileMode.OpenOrCreate);
            using var xlPackage = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var properties = GetWorkbookMetadata<Product>(xlPackage, languages);
            var defaultWorksheet = properties.DefaultWorksheet;
            var defaultProperties = properties.DefaultProperties;
            var localizedProperties = properties.LocalizedProperties;

            var manager = new PropertyManager<Product>(defaultProperties, _catalogSettings, localizedProperties, languages);
            var iRow = productImport.RowNumber == 0 ? 2 : productImport.RowNumber;
            var rowCount = defaultWorksheet.LastRowUsed().RowNumber();
            var maxRow = productImport.RowNumber + _makeTypeModelSettings.HyCapacityModelFitsRecords;

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                {
                    iRow++;
                    if (iRow >= rowCount)
                    {
                        productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;
                        await UpdateProductImportAsync(productImport);
                        return;
                    }
                    else
                        continue;
                }

                var productSku = string.Empty;
                var productStock = string.Empty;
                //Read row data from Xlsx
                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
                foreach (var property in manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "Sku":
                            productSku = property.StringValue.Trim();
                            break;
                        case "Stock":
                            productStock = property.StringValue;
                            break;
                    }
                }

                var product = await _productService.GetProductBySkuAsync(productSku);
                if (product != null && !string.IsNullOrEmpty(productStock))
                {
                    if (productStock == "In-Stock")
                        product.StockQuantity = 10000;
                    else
                        product.StockQuantity = 0;

                    product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                    product.DisplayStockAvailability = true;

                    await _productService.UpdateProductAsync(product);
                }

                //update row processed 
                iRow++;
                productImport.RowNumber = iRow;
                productImport.ImportStatus = ProductImportStatusEnum.Processing;
                await UpdateProductImportAsync(productImport);

                if (iRow >= maxRow)
                    break;
            }

            if (iRow >= rowCount)
                productImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;

            await UpdateProductImportAsync(productImport);
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// Represent a product specification attribute meta data
        /// </summary>
        protected class ProductSpecificationAttributeMetadata
        {
            public int SpecificationAttributeId { get; set; }

            public string SpecificationAttribute { get; set; }

            public int SpecificationAttributeOptionId { get; set; }

            public string SpecificationAttributeOption { get; set; }

        }
        /// <summary>
        /// Represent a category key 
        /// </summary>
        protected partial class CategoryKey
        {
            public CategoryKey(string key, Category category = null, List<int> storesIds = null)
            {
                Key = key.Trim();
                StoresIds = storesIds ?? new List<int>();
                Category = category;
            }

            public List<int> StoresIds { get; }

            public Category Category { get; }

            public string Key { get; }

        }

        #endregion
    }
}