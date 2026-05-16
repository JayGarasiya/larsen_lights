using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.GranitProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelCategory;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents the make type models model factory  
    /// </summary>
    public partial interface IMakeTypeModelFactory
    {
        /// <summary>
        /// Prepare the model product search model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the prepared search model
        /// </returns>
        Task<ModelProductSearchModel> PrepareModelSearchModelAsync(ModelProductSearchModel searchModel);

        /// <summary>
        /// Prepare the make product list model
        /// </summary>
        /// <param name="searchModel">Make product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the make product list model
        /// </returns>
        Task<MakeProductListModel> PrepareMakeProductListModelAsync(MakeProductSearchModel searchModel);

        /// <summary>
        /// Prepare the model-product mapping list model
        /// </summary>
        /// <param name="searchModel">Model product mapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model-product mapping list model
        /// </returns>
        Task<ModelProductMappingListModel> PrepareModelProductMappingListModelAsync(ModelProductMappingSearchModel searchModel);

        /// <summary>
        /// Prepare the model product list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model product list model
        /// </returns>
        Task<ModelProductListModel> PrepareModelProductListModelAsync(ModelProductSearchModel searchModel);

        /// <summary>
        /// Prepare the type product list model
        /// </summary>
        /// <param name="searchModel">Type product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the type product list model
        /// </returns>
        Task<TypeProductListModel> PrepareTypeProductListModelAsync(TypeProductSearchModel searchModel);

        /// <summary>
        /// Prepare the model category list model
        /// </summary>
        /// <param name="searchModel">Model category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list model
        /// </returns>
        Task<ModelCategoryListModel> PrepareCategoryListModelAsync(ModelCategorySearchModel searchModel);

        /// <summary>
        /// Prepare the product import list model
        /// </summary>
        /// <param name="searchModel">Product import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import list model
        /// </returns>
        Task<ProductImportListModel> PrepareProductImportListModelAsync(ProductImportSearchModel searchModel);

        /// <summary>
        /// Prepare the product models search and list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the model product list model
        /// </returns>
        Task<ModelProductListModel> PrepareProductModelsSearchAndListModelAsync(ModelProductSearchModel searchModel);

        /// <summary>
        /// Prepare the product models add popup list model
        /// </summary>
        /// <param name="searchModel">Model product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the popup list model for adding products
        /// </returns>
        Task<ModelProductListModel> PrepareProductModelsAddPopupListModelAsync(ModelProductSearchModel searchModel);

        /// <summary>
        /// Prepare the granit product import list model
        /// </summary>
        /// <param name="searchModel">Granit product import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the granit product import list model
        /// </returns>
        Task<GranitProductImportListModel> PrepareGranitProductImportListModelAsync(GranitProductImportSearchModel searchModel);

        /// <summary>
        /// Prepare the price import list model
        /// </summary>
        /// <param name="searchModel">Price import search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the price import list model
        /// </returns>
        Task<PriceImportListModel> PreparePriceImportsListModelAsync(PriceImportSearchModel searchModel);

        /// <summary>
        /// Prepare the price range import model
        /// </summary>
        /// <param name="searchModel">Price range search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the price range list model
        /// </returns>
        Task<PriceRangeListModel> PreparePriceImportRangeModelAsync(PriceRangeSearchModel searchModel);

        /// <summary>
        /// Prepare the add product to model-product popup list model
        /// </summary>
        /// <param name="searchModel">Add product to model-product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the popup list model
        /// </returns>
        Task<AddProductToModelProductListModel> PrepareModelProductAddPopupListAsync(AddProductToModelProductSearchModel searchModel);

        /// <summary>
        /// Prepare the model-product search model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the prepared search model
        /// </returns>
        Task<ModelProductSearchModel> PrepareProductModelsSearchModelAsync();
    }
}