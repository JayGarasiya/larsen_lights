using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.GranitProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelCategory;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents the make type models model factory  
    /// </summary>
    public partial class MakeTypeModelFactory : IMakeTypeModelFactory
    {
        #region Fields

        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductService _productService;
        protected readonly ICategoryService _categoryService;
        protected readonly IProductImportService _productImportFormExcelService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IGranitProductImportService _granitProductImportService;
        protected readonly IPriceImportService _priceImportService;
        protected readonly IVendorService _vendorService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public MakeTypeModelFactory(IMakeTypeModelService makeTypeModelService,
            ILocalizationService localizationService,
            IProductService productService,
            ICategoryService categoryService,
            IProductImportService productImportFormExcelService,
            IDateTimeHelper dateTimeHelper,
            IGranitProductImportService granitProductImportService,
            IPriceImportService priceImportService,
            IVendorService vendorService,
            IPriceFormatter priceFormatter,
            IUrlRecordService urlRecordService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _makeTypeModelService = makeTypeModelService;
            _localizationService = localizationService;
            _productService = productService;
            _categoryService = categoryService;
            _productImportFormExcelService = productImportFormExcelService;
            _dateTimeHelper = dateTimeHelper;
            _granitProductImportService = granitProductImportService;
            _priceImportService = priceImportService;
            _vendorService = vendorService;
            _priceFormatter = priceFormatter;
            _urlRecordService = urlRecordService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the model category list model
        /// </summary>
        /// <param name="searchModel">Model category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list model
        /// </returns>
        public async Task<ModelCategoryListModel> PrepareCategoryListModelAsync(ModelCategorySearchModel searchModel)
        {
            var modelCategories = await _makeTypeModelService.GetAllModelCategoriesAsync(searchModel.SearchCategoryName, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new ModelCategoryListModel().PrepareToGridAsync(searchModel, modelCategories, () =>
            {
                //fill in model values from the entity
                return modelCategories.SelectAwait(async modelCategory => new ModelCategoryModel
                {
                    Id = modelCategory.Id,
                    CategoryId = modelCategory.CategoryId,
                    CategoryName = (await _categoryService.GetCategoryByIdAsync(modelCategory.CategoryId))?.Name,
                    Published = modelCategory.Published,
                    DisplayOrder = modelCategory.DisplayOrder
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the granit product import list model
        /// </summary>
        /// <param name="searchModel">Granit product import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the granit product import list model
        /// </returns>
        public async Task<GranitProductImportListModel> PrepareGranitProductImportListModelAsync(GranitProductImportSearchModel searchModel)
        {
            //get parameters to filter orders
            var importStatuses = searchModel.SearchImportStatus.Equals(0) ? null : new List<int>() { searchModel.SearchImportStatus };

            var importGranitProducts = await _granitProductImportService.GetAllGranitProductImportAsync(null, importStatuses, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new GranitProductImportListModel().PrepareToGridAsync(searchModel, importGranitProducts, () =>
            {
                //fill in model values from the entity
                return importGranitProducts.SelectAwait(async import =>
                {
                    //fill in model values from the entity
                    var importProd = new GranitProductImportModel
                    {
                        Id = import.Id,
                        FileName = import.FileName,
                        DataRowNumber = import.DataRowNumber,
                    };

                    //convert dates to the user time
                    importProd.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(import.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    importProd.ImportStatus = await _localizationService.GetLocalizedEnumAsync(import.ImportStatus);

                    return importProd;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the make product list model
        /// </summary>
        /// <param name="searchModel">Make product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the make product list model
        /// </returns>
        public async Task<MakeProductListModel> PrepareMakeProductListModelAsync(MakeProductSearchModel searchModel)
        {
            var makeProducts = await _makeTypeModelService.GetAllMakeProductsAsync(searchModel.SearchName, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new MakeProductListModel().PrepareToGrid(searchModel, makeProducts, () =>
            {
                //fill in model values from the entity
                return makeProducts.Select(makeProduct => new MakeProductModel
                {
                    Id = makeProduct.Id,
                    Name = makeProduct.Name,
                    Published = makeProduct.Published,
                    DisplayOrder = makeProduct.DisplayOrder
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the add product to model-product popup list model
        /// </summary>
        /// <param name="searchModel">Add product to model-product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the popup list model
        /// </returns>
        public async Task<AddProductToModelProductListModel> PrepareModelProductAddPopupListAsync(AddProductToModelProductSearchModel searchModel)
        {

            //prepare model
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AddProductToModelProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();

                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the model product list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model product list model
        /// </returns>
        public async Task<ModelProductListModel> PrepareModelProductListModelAsync(ModelProductSearchModel searchModel)
        {
            var modelProducts = await _makeTypeModelService.GetAllModelProductsAsync(searchModel.SearchName, searchModel.SearchInDescription, searchModel.SearchMakeName, searchModel.SearchTypeName,
              searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new ModelProductListModel().PrepareToGrid(searchModel, modelProducts, () =>
            {
                //fill in model values from the entity
                return modelProducts.Select(modelProduct => new ModelProductModel
                {
                    Id = modelProduct.Id,
                    Name = modelProduct.Name,
                    Description = modelProduct.Description,
                    MakeName = modelProduct.MakeName,
                    TypeName = modelProduct.TypeName,
                    Published = modelProduct.Published,
                    DisplayOrder = modelProduct.DisplayOrder
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the model-product mapping list model
        /// </summary>
        /// <param name="searchModel">Model product mapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model-product mapping list model
        /// </returns>
        public async Task<ModelProductMappingListModel> PrepareModelProductMappingListModelAsync(ModelProductMappingSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(searchModel.ModelId)
               ?? throw new ArgumentException("No model product found with the specified id");

            //get model products
            var modelProducts = await _makeTypeModelService.GetAllModelProductMappingsAsync(modelId: modelProduct.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ModelProductMappingListModel().PrepareToGridAsync(searchModel, modelProducts, () =>
            {
                return modelProducts.SelectAwait(async modelProduct =>
                {
                    //fill in model values from the entity
                    var modelProductModel = new ModelProductMappingModel()
                    {
                        Id = modelProduct.Id,
                        ModelId = modelProduct.ModelId,
                        ProductId = modelProduct.ProductId
                    };

                    //fill in additional values (not existing in the entity)
                    modelProductModel.ProductName = (await _productService.GetProductByIdAsync(modelProduct.ProductId))?.Name;

                    return modelProductModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare the model product search model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the prepared search model
        /// </returns>
        public async Task<ModelProductSearchModel> PrepareModelSearchModelAsync(ModelProductSearchModel searchModel)
        {
            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                searchModel.AvailableMakes.Add(new SelectListItem
                {
                    Text = make.Name,
                    Value = make.Name
                });

            searchModel.AvailableMakes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = string.Empty
            });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                searchModel.AvailableTypes.Add(new SelectListItem
                {
                    Text = type.Name,
                    Value = type.Name
                });

            searchModel.AvailableTypes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = string.Empty
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare the price range import model
        /// </summary>
        /// <param name="searchModel">Price range search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the price range list model
        /// </returns>
        public async Task<PriceRangeListModel> PreparePriceImportRangeModelAsync(PriceRangeSearchModel searchModel)
        {
            var priceImport = await _priceImportService.GetPriceImportByIdAsync(searchModel.PriceImportId)
                ?? throw new ArgumentException("No data found with the specified id");
            //prepare model
            searchModel.SetGridPageSize();
            var priceRangeList = string.IsNullOrEmpty(priceImport.PriceRange) ? new List<PriceRangeJson>() : JsonConvert.DeserializeObject<List<PriceRangeJson>>(priceImport.PriceRange);

            var pagedList = priceRangeList.ToPagedList(searchModel);
            //prepare list model
            var model = await new PriceRangeListModel().PrepareToGridAsync(searchModel, pagedList, () =>
            {
                //fill in model values from the entity
                return pagedList.SelectAwait(async item =>
                {
                    //fill in model values from the entity
                    var priceRangeModel = new PriceRangeModel
                    {
                        PricePercentage = $"{item.PricePercentage} %",
                        PriceAmount = await _priceFormatter.FormatPriceAsync(item.PriceAmount, true, false),
                        FromPrice = await _priceFormatter.FormatPriceAsync(item.FromPrice, true, false),
                        ToPrice = await _priceFormatter.FormatPriceAsync(item.ToPrice, true, false)
                    };

                    return priceRangeModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the price import list model
        /// </summary>
        /// <param name="searchModel">Price import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the price import list model
        /// </returns>
        public async Task<PriceImportListModel> PreparePriceImportsListModelAsync(PriceImportSearchModel searchModel)
        {
            //get parameters to filter orders
            var importStatuses = searchModel.SearchImportStatus.Equals(0) ? null : new List<int>() { searchModel.SearchImportStatus };

            var importPriceProducts = await _priceImportService.GetAllPriceImportAsync(importStatuses, searchModel.Page - 1, searchModel.PageSize, vendorId: searchModel.VendorId);

            //prepare list model
            var model = await new PriceImportListModel().PrepareToGridAsync(searchModel, importPriceProducts, () =>
            {
                //fill in model values from the entity
                return importPriceProducts.SelectAwait(async import =>
                {
                    //fill in model values from the entity
                    var importProd = new PriceImportModel
                    {
                        Id = import.Id,
                        FileName = import.FileName,
                        RowNumber = import.RowNumber,
                        VendorId = import.VendorId,
                        VendorName = (await _vendorService.GetVendorByIdAsync(import.VendorId)).Name,
                        DownloadId = import.DownloadId
                    };

                    //convert dates to the user time
                    importProd.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(import.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    importProd.ImportStatus = await _localizationService.GetLocalizedEnumAsync(import.ImportStatus);

                    return importProd;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the product import list model
        /// </summary>
        /// <param name="searchModel">Product import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import list model
        /// </returns>
        public async Task<ProductImportListModel> PrepareProductImportListModelAsync(ProductImportSearchModel searchModel)
        {
            //get parameters to filter orders
            var importStatuses = searchModel.SearchImportStatus.Equals(0) ? null : new List<int>() { searchModel.SearchImportStatus };
            var importTypes = searchModel.SearchImportType.Equals(0) ? null : new List<int>() { searchModel.SearchImportType };

            var importProducts = await _productImportFormExcelService.GetAllProductImportAsync(null, importTypes, importStatuses, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new ProductImportListModel().PrepareToGridAsync(searchModel, importProducts, () =>
            {
                //fill in model values from the entity
                return importProducts.SelectAwait(async import =>
                {
                    //fill in model values from the entity
                    var importProd = new ProductImportModel
                    {
                        Id = import.Id,
                        FileName = import.FileName,
                        RowNumber = import.RowNumber,
                        DeleteAll = import.DeleteAll
                    };

                    //convert dates to the user time
                    importProd.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(import.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    importProd.ImportStatus = await _localizationService.GetLocalizedEnumAsync(import.ImportStatus);
                    importProd.ImportType = await _localizationService.GetLocalizedEnumAsync(import.ImportType);

                    return importProd;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the product models add popup list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the popup list model for adding products
        /// </returns>
        public async Task<ModelProductListModel> PrepareProductModelsAddPopupListModelAsync(ModelProductSearchModel searchModel)
        {
            //get models product
            var modelProducts = await _makeTypeModelService.GetAllModelProductsAsync(searchModel.SearchName, searchModel.SearchInDescription, searchModel.SearchMakeName, searchModel.SearchTypeName,
                searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new ModelProductListModel().PrepareToGrid(searchModel, modelProducts, () =>
            {
                //fill in model values from the entity
                return modelProducts.Select(modelProduct => new ModelProductModel
                {
                    Id = modelProduct.Id,
                    Name = modelProduct.Name,
                    Description = modelProduct.Description,
                    MakeName = modelProduct.MakeName,
                    TypeName = modelProduct.TypeName,
                    Published = modelProduct.Published,
                    DisplayOrder = modelProduct.DisplayOrder
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the product models search and list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model product list model
        /// </returns>
        public async Task<ModelProductListModel> PrepareProductModelsSearchAndListModelAsync(ModelProductSearchModel searchModel)
        {
            searchModel.SetGridPageSize();

            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);

            //get model products
            var modelProducts = await _makeTypeModelService.GetAllModelProductMappingsAsync(
                modelId: 0,
                productId: product.Id,
                searchModel.SearchName,
                searchModel.SearchInDescription,
                searchModel.SearchMakeName,
                searchModel.SearchTypeName,
                searchModel.Page - 1,
                searchModel.PageSize
            );

            //prepare list model
            var model = await new ModelProductListModel().PrepareToGridAsync(searchModel, modelProducts, () =>
            {
                return modelProducts.SelectAwait(async modelProduct =>
                {
                    var modelInfo = await _makeTypeModelService
                        .GetModelProductByIdAsync(modelProduct.ModelId);

                    return new ModelProductModel
                    {
                        Id = modelProduct.Id,
                        Name = modelInfo.Name,
                        Description = modelInfo.Description,
                        MakeName = modelInfo.MakeName,
                        TypeName = modelInfo.TypeName,
                        Published = modelInfo.Published,
                        DisplayOrder = modelInfo.DisplayOrder
                    };
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare the type product list model
        /// </summary>
        /// <param name="searchModel">Type product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the type product list model
        /// </returns>
        public async Task<TypeProductListModel> PrepareTypeProductListModelAsync(TypeProductSearchModel searchModel)
        {
            var typeProducts = await _makeTypeModelService.GetAllTypeProductsAsync(searchModel.SearchName, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new TypeProductListModel().PrepareToGrid(searchModel, typeProducts, () =>
            {
                //fill in model values from the entity
                return typeProducts.Select(typeProduct => new TypeProductModel
                {
                    Id = typeProduct.Id,
                    Name = typeProduct.Name,
                    Published = typeProduct.Published,
                    DisplayOrder = typeProduct.DisplayOrder
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare the product search model 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the type product list model
        /// </returns>
        public async Task<ModelProductSearchModel> PrepareProductModelsSearchModelAsync()
        {
            var model = new ModelProductSearchModel();

            // Makes
            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
            {
                model.AvailableMakes.Add(new SelectListItem
                {
                    Text = make.Name,
                    Value = make.Name
                });
            }

            model.AvailableMakes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = string.Empty
            });

            // Types
            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
            {
                model.AvailableTypes.Add(new SelectListItem
                {
                    Text = type.Name,
                    Value = type.Name
                });
            }

            model.AvailableTypes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = string.Empty
            });

            // Grid settings
            model.SetGridPageSize();

            return model;
        }

        #endregion
    }
}
