using Nop.Plugin.Reports.ChartsAndTables.Models;

namespace Nop.Plugin.Reports.ChartsAndTables.Factories
{
    /// <summary>
    /// Represents the charts and tables model factory
    /// </summary>
    public partial interface IChartsAndTablesModelFactory
    {
        #region Product

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportMenuProductSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductMenuProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductMenuProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu product attribute list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product attribute list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductMenuProductAttributeListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu product attribute aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product attribute aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductMenuProductAttributeAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu category list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductMenuCategoryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu category aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductMenuCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu manufacturer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductMenuManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu manufacturer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductMenuManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu vendor list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductMenuVendorListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product menu vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu vendor aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductMenuVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportDetailProductSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail product attribute list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product attribute list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductDetailProductAttributeListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail product attribute aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product attribute aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductDetailProductAttributeAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail customer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductDetailCustomerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail customer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductDetailCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail country list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail country list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductDetailCountryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail country aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail country aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareProductDetailCountryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare product detail global list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail global list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareProductDetailGlobalListModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Customer

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportMenuCustomerSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerMenuCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer registered list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer registered list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerRegisteredListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer registered aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer registered aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerMenuCustomerRegisteredAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer gender list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer gender list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerGenderListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer gender aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer gender aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerMenuCustomerGenderAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer role list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer role list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerRoleListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu customer role aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer role aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerMenuCustomerRoleAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu country list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu country list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCountryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer menu country aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu country aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerMenuCountryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportDetailCustomerSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail customer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerDetailCustomerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail customer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerDetailCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail category list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail category aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail manufacturer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail manufacturer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail vendor list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCustomerDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare customer detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail vendor aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCustomerDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Order 

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportMenuOrderSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order sales in time list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu order sales in time list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareOrderMenuOrderListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order sales in time aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu sales in time aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareOrderMenuOrderAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order item sales in time list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu order sales in time list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareOrderMenuOrderItemListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order item sales in time aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu sales in time aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareOrderMenuOrderItemAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order shipping method list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu shipping method list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareOrderMenuShippingMethodListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu shipping method aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu shipping method aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareOrderMenuShippingMethodAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu order payment method list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu payment method list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareOrderMenuPaymentMethodListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare order menu payment method aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu payment method aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareOrderMenuPaymentMethodAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Table

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportMenuTableSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare table report list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the table report list model
        /// </returns>
        Task<TableReportListModel> PrepareTableReportListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare table report aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the table report aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareTableReportAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Category

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportDetailCategorySearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail category list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCategoryDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail category aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCategoryDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCategoryDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCategoryDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail manufacturer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCategoryDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail manufacturer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCategoryDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail vendor list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCategoryDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail vendor aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCategoryDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail categories share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail categories share list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareCategoryDetailCategoriesShareListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare category detail categories share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail categories share aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareCategoryDetailCategoriesShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Manufacturer

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportDetailManufacturerSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareManufacturerDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareManufacturerDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail category list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail category aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareManufacturerDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail manufacturers share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturers share list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailManufacturersShareListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare manufacturer detail manufacturers share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturers share aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareManufacturerDetailManufacturersShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion

        #region Vendor

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        Task<ChartsAndTablesSearchModel> PrepareReportDetailVendorSearchModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendor list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareVendorDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendor aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareVendorDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail product list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareVendorDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail product aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareVendorDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail category list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareVendorDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail category aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareVendorDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail manufacturer list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareVendorDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail manufacturer aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareVendorDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail vendors share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendors share list model
        /// </returns>
        Task<ChartsAndTablesDataListModel> PrepareVendorDetailVendorsShareListModelAsync(ChartsAndTablesSearchModel searchModel);

        /// <summary>
        /// Prepare vendor detail vendors share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendors share aggregator model
        /// </returns>
        Task<AggreratorModel> PrepareVendorDetailVendorsShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel);

        #endregion
    }
}
