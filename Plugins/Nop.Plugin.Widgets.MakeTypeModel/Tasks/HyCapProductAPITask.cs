using LinqToDB;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.BaseApiCall;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Models.HyCapacityAPI;
using Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.Http.Headers;
using Category = Nop.Core.Domain.Catalog.Category;

namespace Nop.Plugin.Widgets.MakeTypeModel.Tasks
{
    /// <summary>
    /// Represents a schedule task of hy-cap product API
    /// </summary>
    public class HyCapProductAPITask : IScheduleTask
    {
        #region Fields

        protected readonly ISettingService _settingService;
        protected readonly IProductService _productService;
        protected readonly IProductTagService _productTagService;
        protected readonly INopFileProvider _fileProvider;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger _logger;
        protected readonly MediaSettings _mediaSettings;
        protected readonly IPictureService _pictureService;
        protected readonly INopDataProvider _dataProvider;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly ISpecificationAttributeService _specificationAttributeService;
        protected readonly ICategoryService _categoryService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly ICustomerService _customerService;
        protected readonly IAclService _aclService;
        protected readonly IStoreService _storeService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly IStoreContext _storeContext;
        protected readonly IGranitProductImportService _granitProductImportService;
        protected readonly IPriceImportService _priceImportService;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;

        #endregion

        #region Ctor
        public HyCapProductAPITask(ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            INopFileProvider fileProvider,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            MediaSettings mediaSettings,
            IPictureService pictureService,
            INopDataProvider dataProvider,
            IUrlRecordService urlRecordService,
            IMakeTypeModelService makeTypeModelService,
            ISpecificationAttributeService specificationAttributeService,
            ICategoryService categoryService,
            IStoreMappingService storeMappingService,
            ILocalizedEntityService localizedEntityService,
            ICustomerService customerService,
            IAclService aclService,
            IStoreService storeService,
            IHtmlFormatter htmlFormatter,
            IStoreContext storeContext,
            IGranitProductImportService granitProductImportService,
            MakeTypeModelSettings makeTypeModelSettings,
            IPriceImportService priceImportService)
        {
            _settingService = settingService;
            _productService = productService;
            _productTagService = productTagService;
            _httpClientFactory = httpClientFactory;
            _fileProvider = fileProvider;
            _logger = logger;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            _dataProvider = dataProvider;
            _urlRecordService = urlRecordService;
            _makeTypeModelService = makeTypeModelService;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _storeMappingService = storeMappingService;
            _localizedEntityService = localizedEntityService;
            _customerService = customerService;
            _aclService = aclService;
            _storeService = storeService;
            _htmlFormatter = htmlFormatter;
            _storeContext = storeContext;
            _granitProductImportService = granitProductImportService;
            _makeTypeModelSettings = makeTypeModelSettings;
            _priceImportService = priceImportService;
        }

        #endregion

        #region Constants

        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA512";

        private const string UPLOADS_TEMP_PATH = "~/App_Data/TempUploads";

        #endregion

        #region Utilities

        /// <summary>
        /// Product Update API
        /// </summary>
        /// <param name="authorizationToken">Authorization Token</param>
        /// <param name="tokenGuidId">Token Guide Indentifier</param>
        /// <param name="apiUrl">Api Url</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="pageNumber">Page Number</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<ProductAPIResponseModel> ProductsUpdateAPIAsync(string authorizationToken, string tokenGuidId, string apiUrl, string pageSize, string pageNumber)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                tokenGuidId = tokenGuidId,
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _logger.ErrorAsync("ProductsUpdateAPI failed", new Exception(responseJson));
                return null;
                //throw new HttpRequestException($"Product API request failed: {error}");
            }

            var result = JsonConvert.DeserializeObject<ProductAPIResponseModel>(responseJson);

            return result;
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

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
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
        private async Task<Category> FindDeepestMatchingCategoryAsync(List<CategoryDto> orderedCategories, int rootCategoryId)
        {
            var parentId = rootCategoryId;
            var matchedCategory = new Category();

            foreach (var level in orderedCategories)
            {
                var children = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentId, showHidden: true);
                matchedCategory = children
                    .FirstOrDefault(c => c.Name.Equals(level.CategoryName?.Trim(), StringComparison.InvariantCultureIgnoreCase));

                if (matchedCategory == null)
                    return null;

                parentId = matchedCategory.Id;
            }

            return matchedCategory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<MakeTypeModelSettings>(storeId);
            var authAPIUrl = settings.AuthorizationAPIURL;
            var inventoryAPIUrl = settings.InventoryAPIURL;
            var userName = settings.Username;
            var passWord = settings.Password;
            var tokenGuidId = settings.TokenGuidId;
            var lastProductAPIExecutedDate = settings.LastExecutedProductAPI;
            var inventoryObject = new InventoryAPIResponseModel();

            var priceRangeList = new List<PriceRangeJson>();
            var matchedRange = new PriceRangeJson();

            if (!lastProductAPIExecutedDate.HasValue)
                return;

            var lastRunDate = lastProductAPIExecutedDate.Value.Date;
            var now = DateTime.UtcNow.Date;

            // Get week numbers
            var lastRunWeek = ISOWeek.GetWeekOfYear(lastRunDate);
            var currentWeek = ISOWeek.GetWeekOfYear(now);

            // Also compare year (week 1 issue)
            var lastRunYear = ISOWeek.GetYear(lastRunDate);
            var currentYear = ISOWeek.GetYear(now);

            // If already executed in same week, return
            var alreadyExecutedThisWeek = lastRunWeek == currentWeek && lastRunYear == currentYear;

            if (alreadyExecutedThisWeek)
                return;

            settings.ProductUpdateAPICurrentPage++;

            if (string.IsNullOrEmpty(settings.AuthorizationAPIURL) ||
                string.IsNullOrEmpty(settings.Username) ||
                string.IsNullOrEmpty(settings.Password) ||
                string.IsNullOrEmpty(settings.ProductsAPIURL) ||
                string.IsNullOrEmpty(settings.TokenGuidId))
                return;

            if (settings.ProductUpdateAPICurrentPage <= 0 || settings.ProductUpdateAPINoOfPages <= 0 || settings.ProductUpdateAPIPagesize <= 0)
                return;

            //var isExpired = !settings.LastExecutedProductAPI.HasValue || settings.LastExecutedProductAPI.Value <= DateTime.UtcNow.AddDays(-1);
            //if (!isExpired)
            //    return;

            if (settings.ProductUpdateAPICurrentPage > settings.ProductUpdateAPINoOfPages)
                return;

            var token = await HyCapacityAPI.GetAuthorizationTokenAsync(settings.AuthorizationAPIURL, settings.Username, settings.Password);

            if (string.IsNullOrEmpty(token))
                return;

            var inventoryAPIResponse = await HyCapacityAPI.InventoryAPIAsync(authorizationToken: token,
                                           tokenGuidId: tokenGuidId, apiUrl: inventoryAPIUrl);

            var productAPI = await ProductsUpdateAPIAsync(
                authorizationToken: token,
                tokenGuidId: settings.TokenGuidId,
                apiUrl: settings.ProductsAPIURL,
                pageSize: settings.ProductUpdateAPIPagesize.ToString(),
                pageNumber: settings.ProductUpdateAPICurrentPage.ToString());

            if(!string.IsNullOrEmpty(inventoryAPIResponse))
                 inventoryObject = JsonConvert.DeserializeObject<InventoryAPIResponseModel>(inventoryAPIResponse);

            var inventoryDict = inventoryObject?.Payload?
                .GroupBy(x => x.HyCapStockNumber, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToDictionary(x => x.HyCapStockNumber, StringComparer.OrdinalIgnoreCase);

            if (productAPI == null || !productAPI.IsSuccessStatusCode)
                return;

            var priceImport = (await _priceImportService.GetAllPriceImportAsync(vendorId: _makeTypeModelSettings.HyCapacityVendorId)).FirstOrDefault();

            if (priceImport != null)
                priceRangeList = string.IsNullOrEmpty(priceImport.PriceRange) ? new List<PriceRangeJson>() : JsonConvert.DeserializeObject<List<PriceRangeJson>>(priceImport.PriceRange);

            if (productAPI.Pager != null)
            {
                settings.ProductUpdateAPINoOfPages = productAPI.Pager.TotalPages;
                settings.ProductUpdateAPIPagesize = productAPI.Pager.PageSize;
            }

            // Split exclude categories from settings
            var excludedCategories = (settings.ExcludeCategory ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim().ToLower()).ToList();

            var productSkus = productAPI.Payload
                .Where(p =>
                    !string.IsNullOrEmpty(p.HyCapStockNumber)
                ).Select(p => p.HyCapStockNumber).Distinct().ToArray();

            var existingProducts = (await _granitProductImportService.GetProductsBySkuAsync(productSkus))
                                   .GroupBy(p => p.Sku, StringComparer.OrdinalIgnoreCase)
                                   .Select(g => g.First())
                                   .ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

            var excludeProductSkus = productAPI.Payload
                .Where(p =>
                    !string.IsNullOrEmpty(p.HyCapStockNumber) &&
                    p.Categories != null && p.Categories.Any() && excludedCategories.Any(ex => string.Equals(p.Categories.Last().CategoryName, ex, StringComparison.OrdinalIgnoreCase))
                ).Select(p => p.HyCapStockNumber).Distinct().ToArray();

            foreach (var item in productAPI.Payload)
            {
                if (string.IsNullOrEmpty(item.HyCapStockNumber))
                    continue;

                var isNew = !existingProducts.TryGetValue(item.HyCapStockNumber, out var product);
                product ??= new Product();

                if (item.ProductName != null)
                {
                    product.Published = true;
                    product.Name = item.ProductName;
                    product.Sku = item.HyCapStockNumber;

                    if (isNew)
                    {
                        if (!excludedCategories.Contains(item.Categories.Last().CategoryName.ToLower()))
                        {
                            inventoryDict.TryGetValue(item.HyCapStockNumber, out var inv);
                            var dealerPrice = inv?.DealerPrice ?? 0m;

                            var aPIPrice = dealerPrice > 0 ? dealerPrice : item.SuggListPrice;
                            var updatedPrice = aPIPrice;

                            product.CreatedOnUtc = DateTime.UtcNow;
                            product.OrderMinimumQuantity = 1;
                            product.OrderMaximumQuantity = 10000;
                            product.ProductType = ProductType.SimpleProduct;
                            product.VisibleIndividually = true;
                            product.AllowCustomerReviews = true;
                            product.IsShipEnabled = true;
                            product.Published = true;
                            product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                            product.StockQuantity = 10000;

                            await _productService.InsertProductAsync(product);

                            if (inventoryObject != null && priceRangeList != null && priceRangeList.Any())
                            {
                                matchedRange = priceRangeList
                                    .FirstOrDefault(r => aPIPrice >= r.FromPrice && aPIPrice <= r.ToPrice);

                                if (matchedRange != null)
                                {
                                    if (matchedRange.UsePercentage)
                                    {
                                        updatedPrice = aPIPrice + (aPIPrice * matchedRange.PricePercentage / 100m);
                                    }
                                    else
                                    {
                                        updatedPrice = aPIPrice + matchedRange.PriceAmount;
                                    }
                                }
                                else
                                {
                                    if (product.Price == 0 && dealerPrice > 0)
                                        updatedPrice = dealerPrice;
                                    else
                                        updatedPrice = product.Price;
                                }
                            }

                            updatedPrice = Math.Round(updatedPrice, 2);
                            product.ProductCost = aPIPrice;
                            product.Price = updatedPrice;

                            if (!string.IsNullOrEmpty(item.ProductImage?.Trim()))
                            {
                                var imageUrl = item.ProductImage.Trim();
                                var downloadedFiles = new List<string>();
                                await DownloadFileAsync(imageUrl, downloadedFiles);

                                if (_mediaSettings.ImportProductImagesUsingHash && await _pictureService.IsStoreInDbAsync())
                                    await ImportProductImagesUsingHashAsync(downloadedFiles.ToArray(), product, true);
                                else
                                    await ImportProductImagesUsingServicesAsync(downloadedFiles.ToArray(), product, true);

                                foreach (var file in downloadedFiles)
                                {
                                    if (_fileProvider.FileExists(file))
                                    {
                                        try
                                        { _fileProvider.DeleteFile(file); }
                                        catch { }
                                    }
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        await _productService.UpdateProductAsync(product);
                    }

                    if (settings.HyCapacityCategoryId > 0 && product != null && item.Categories?.Any() == true)
                    {
                        var parentCategoryId = settings.HyCapacityCategoryId;

                        // Ensure the categories are ordered by level
                        var orderedCategories = item.Categories.OrderBy(c => c.CategoryLevel).ToList();

                        // Try to get final category name
                        var finalCategoryName = orderedCategories.LastOrDefault()?.CategoryName?.Trim();
                        if (string.IsNullOrEmpty(finalCategoryName))
                            continue;

                        // Check if final category exists under hierarchy (fast exit optimization)
                        var finalCategory = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId, showHidden: true);
                        var matchedFinalCategory = await FindDeepestMatchingCategoryAsync(orderedCategories, settings.HyCapacityCategoryId);
                        if (matchedFinalCategory != null)
                        {
                            var productCategory = new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = matchedFinalCategory.Id,
                                IsFeaturedProduct = false,
                                DisplayOrder = 0
                            };
                            await _categoryService.InsertProductCategoryAsync(productCategory);
                        }
                        else
                        {
                            // If not found, create or walk through the hierarchy
                            foreach (var categoryLevel in orderedCategories)
                            {
                                var categoryName = categoryLevel.CategoryName?.Trim();
                                if (string.IsNullOrEmpty(categoryName))
                                    continue;

                                // Get or create child category
                                var existingCategory = (await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId, showHidden: true))
                                    .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));

                                var category = new Category();
                                if (existingCategory != null)
                                {
                                    category = existingCategory;
                                }
                                else
                                {
                                    category = new Category
                                    {
                                        Name = categoryName,
                                        ParentCategoryId = parentCategoryId,
                                        Published = true,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        UpdatedOnUtc = DateTime.UtcNow
                                    };
                                    await _categoryService.InsertCategoryAsync(category);
                                }

                                parentCategoryId = category.Id;
                            }

                            var productCategory = new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = parentCategoryId,
                                IsFeaturedProduct = false,
                                DisplayOrder = 0
                            };
                            await _categoryService.InsertProductCategoryAsync(productCategory);
                        }
                    }

                    if (excludedCategories.Contains(item.Categories.Last().CategoryName.ToLower()))
                    {
                        var excludeProduct = await _productService.GetProductBySkuAsync(item.HyCapStockNumber);
                        if (excludeProduct != null)
                        {
                            excludeProduct.Published = false;
                            excludeProduct.UpdatedOnUtc = DateTime.UtcNow;

                            var category = await _granitProductImportService.GetCategoryByNameAsync(categoryName: item.Categories.Last().CategoryName);
                            if (category != null)
                            {
                                var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id, showHidden: true);
                                foreach(var productCategory in productCategories)
                                {
                                    if(productCategory != null)
                                    {
                                        await _categoryService.DeleteProductCategoryAsync(productCategory);
                                    }
                                }
                                category.Published = false;
                                await _categoryService.UpdateCategoryAsync(category);
                            }

                            await _productService.UpdateProductAsync(excludeProduct);
                            continue;
                        }
                    }

                    if (settings.HyCapacityVendorId > 0)
                        product.VendorId = settings.HyCapacityVendorId;

                    if (settings.HyCapacityWarehouseId > 0)
                        product.WarehouseId = settings.HyCapacityWarehouseId;

                    product.UpdatedOnUtc = DateTime.UtcNow;

                    var seName = isNew ? string.Empty : await _urlRecordService.GetSeNameAsync(product, 0);
                    await _urlRecordService.SaveSlugAsync(product, await _urlRecordService.ValidateSeNameAsync(product, seName, product.Name, true), 0);

                    if (!string.IsNullOrEmpty(item.ProductNotes))
                        product.ShortDescription = _htmlFormatter.ConvertHtmlToPlainText(item.ProductNotes, true, true);

                    if (!string.IsNullOrEmpty(item.XRefNumbers))
                    {
                        var productTags = item.XRefNumbers.Split(',').Select(i => i.Trim()).ToArray();
                        await _productTagService.UpdateProductTagsAsync(product, productTags);
                    }

                    // Dimensions
                    product.Height = item.Height ?? product.Height;
                    product.Weight = item.Weight ?? product.Weight;
                    product.Length = item.Length ?? product.Length;
                    product.Width = item.Width ?? product.Width;

                    //create or update model fit product
                    if (item.Fits.Count > 0)
                    {
                        foreach (var fit in item.Fits)
                        {
                            var modelProduct = new ModelProduct();

                            modelProduct.MakeName = fit.Make;
                            modelProduct.TypeName = fit.Type;
                            modelProduct.Name = fit.Model;
                            modelProduct.Description = string.IsNullOrEmpty(fit.FitNote) || fit.FitNote.Equals("null") ? null : fit.FitNote;

                            if (product != null && !string.IsNullOrEmpty(modelProduct.Name) && !string.IsNullOrEmpty(modelProduct.MakeName) && !string.IsNullOrEmpty(modelProduct.TypeName))
                            {

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
                        }
                    }

                    // create or update product specification attrbutes
                    if (item.ProductSpecs.Count > 0)
                    {
                        //product specification attribute
                        var productSpecificationAttributes = new List<ProductSpecificationAttributeMetadata>();
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

                        foreach (var specification in item.ProductSpecs)
                        {

                            var specNameString = specification.AttributeName.Trim();
                            var specValueString = specification.AttributeValue.Trim();

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
                                    var specificationAttribute = await _granitProductImportService.GetSpecificationAttributeByNameAsync(specNameString);
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
                        }
                    }

                    await _productService.UpdateProductAsync(product);
                }
            }

            if (settings.ProductUpdateAPICurrentPage == settings.ProductUpdateAPINoOfPages)
            {
                settings.LastExecutedProductAPI = DateTime.UtcNow;
                settings.ProductUpdateAPICurrentPage = 0;

                var thresholdDate = DateTime.UtcNow.AddDays(-7);

                var outdatedProducts = (await _productService.SearchProductsAsync(warehouseId: settings.HyCapacityWarehouseId))
                                        .Where(p => p.UpdatedOnUtc < thresholdDate).ToList();

                foreach (var product in outdatedProducts)
                {
                    product.Published = false;
                    product.UpdatedOnUtc = DateTime.UtcNow;

                    await _productService.UpdateProductAsync(product);
                }
            }

            await _settingService.SaveSettingAsync(settings);
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// Represents a product specification attribute metadata
        /// </summary>
        protected class ProductSpecificationAttributeMetadata
        {
            public int SpecificationAttributeId { get; set; }

            public string SpecificationAttribute { get; set; }

            public int SpecificationAttributeOptionId { get; set; }

            public string SpecificationAttributeOption { get; set; }

        }

        #endregion
    }
}
