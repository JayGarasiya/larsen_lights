using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents the find parts model factory  
    /// </summary>
    public partial interface IFindPartsModelFactory
    {
        #region Searching

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        Task<Models.SearchModel> PrepareSearchModelAsync(Models.SearchModel model, CatalogProductsCommand command);

        /// <summary>
        /// Prepares the search products model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search products model
        /// </returns>
        Task<CatalogProductsModel> PrepareSearchProductsModelAsync(Models.SearchModel searchModel, CatalogProductsCommand command);

        #endregion

        #region Common

        /// <summary>
        /// Prepare sorting options
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareSortingOptionsAsync(CatalogProductsModel pagingFilteringModel, CatalogProductsCommand command);

        /// <summary>
        /// Prepare view modes
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareViewModesAsync(CatalogProductsModel pagingFilteringModel, CatalogProductsCommand command);

        /// <summary>
        /// Prepare page size options
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <param name="allowCustomersToSelectPageSize">Are customers allowed to select page size?</param>
        /// <param name="pageSizeOptions">Page size options</param>
        /// <param name="fixedPageSize">Fixed page size</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PreparePageSizeOptionsAsync(CatalogProductsModel pagingFilteringModel, CatalogProductsCommand command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize);

        #endregion
    }
}