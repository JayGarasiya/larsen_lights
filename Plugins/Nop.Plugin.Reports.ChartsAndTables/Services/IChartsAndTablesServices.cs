using Nop.Core;
using Nop.Plugin.Reports.ChartsAndTables.Domain;

namespace Nop.Plugin.Reports.ChartsAndTables.Services
{
    /// <summary>
    /// Charts and tables service interface
    /// </summary>
    public partial interface IChartsAndTablesServices
    {
        /// <summary>
        /// Check License Key Valid 
        /// </summary>
        /// <param name="licenseKey">licenseKey</param>
        /// <returns>Status code</returns>
        Task<int> CheckLicenseKeyValidAsync(string licenseKey);

        /// <summary>
        /// Check whether the plugin is active for the current customer and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<bool> PluginActiveAsync();

        #region Dashboard

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardWeekStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardYearStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="lastNYear">A value in indicating whether you want to load only total number of last n years records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardYearMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int lastNYear, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardBestsellerProductsYearStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="lastNMonth">A value in indicating whether you want to load only total number of last n months records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardBestsellerProductsLastNMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, int lastNMonth, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardBestsellerCategoriesStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardBestsellerManufacturersStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax);

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        Task<List<object>> LoadDashboardBestsellerVendorsStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax);

        #endregion

        #region Product

        /// <summary>
        /// Load product menu product data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductMenuProductDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product menu product aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductMenuProductAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product menu product attribute data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductMenuProductAttributeDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product menu product attribute aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductMenuProductAttributeAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product menu category data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductMenuCategoryDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product menu category aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductMenuCategoryAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product menu manufacturer data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductMenuManufacturerDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product menu manufacturer aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductMenuManufacturerAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product menu vendor data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductMenuVendorDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product menu vendor aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductMenuVendorAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product detail product data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductDetailProductDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product detail product aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductDetailProductAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load product detail product attribute data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductDetailProductAttributeDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product detail product attribute aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductDetailProductAttributeAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product detail customer data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductDetailCustomerDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product detail customer aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductDetailCustomerAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product detail country data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductDetailCountryDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product detail country aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadProductDetailCountryAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load product detail global data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadProductDetailGlobalDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Customer

        /// <summary>
        /// Load customer menu customer data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer menu customer aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerMenuCustomerAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer menu customer registered  data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupId">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerRegisteredDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer menu customer registered aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupId">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerMenuCustomerRegisteredAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load customer menu customer gender data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerGenderDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer menu customer gender aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerMenuCustomerGenderAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer menu customer role data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerRoleDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer menu customer role aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerMenuCustomerRoleAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer menu country data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCountryDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer menu country aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerMenuCountryAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer detail customer data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailCustomerDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer detail customer aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerDetailCustomerAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load customer detail product data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailProductDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer detail product aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerDetailProductAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer detail category data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailCategoryDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer detail category aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerDetailCategoryAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer detail manufacturer data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailManufacturerDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer detail manufacturer aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerDetailManufacturerAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load customer detail vendor data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailVendorDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get customer detail vendor aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCustomerDetailVendorAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        #endregion

        #region Order 

        /// <summary>
        /// Load Order menu order sales in time data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuOrderDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get order menu order sales in time aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadOrderMenuOrderAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false);

        /// <summary>
        /// Load Order menu order item sales in time data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuOrderItemDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get order menu order item sales in time aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadOrderMenuOrderItemAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false);

        /// <summary>
        /// Load Order menu order shipping method data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuShippingMethodDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get order menu order shipping method aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadOrderMenuShippingMethodAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false);

        /// <summary>
        /// Load Order menu order payment method data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuPaymentMethodDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get order menu order payment method aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadOrderMenuPaymentMethodAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false);


        #endregion

        #region Table

        /// <summary>
        /// Load table report
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<TableReport>> LoadTableReportAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get table report aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadTableReportAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0);

        #endregion

        #region Category

        /// <summary>
        /// Load category detail category data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailCategoryDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get category detail category aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCategoryDetailCategoryAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load category detail product data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailProductDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get category detail product aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCategoryDetailProductAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load category detail manufacturer data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailManufacturerDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get category detail manufacturer aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCategoryDetailManufacturerAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load category detail vendor data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailVendorDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get category detail vendor aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCategoryDetailVendorAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load category detail categories share data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailCategoriesShareDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get category detail categories share aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadCategoryDetailCategoriesShareAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false);

        #endregion

        #region Manufacturer

        /// <summary>
        /// Load manufacturer detail manufacturer data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailManufacturerDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get manufacturer detail manufacturer aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadManufacturerDetailManufacturerAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load manufacturer detail product data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailProductDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get manufacturer detail product aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadManufacturerDetailProductAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load manufacturer detail category data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailCategoryDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get manufacturer detail category aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadManufacturerDetailCategoryAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load manufacturer detail manufacturers share data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailManufacturersShareDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get manufacturer detail manufacturers share aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadManufacturerDetailManufacturersShareAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false);

        #endregion

        #region Vendor

        /// <summary>
        /// Load vendor detail vendor data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailVendorDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get vendor detail vendor aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadVendorDetailVendorAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false);

        /// <summary>
        /// Load vendor detail product data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailProductDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get vendor detail product aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadVendorDetailProductAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load vendor detail category data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailCategoryDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get vendor detail category aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadVendorDetailCategoryAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load vendor detail manufacturer data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailManufacturerDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get vendor detail manufacturer aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadVendorDetailManufacturerAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false);

        /// <summary>
        /// Load vendor detail vendors share data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailVendorsShareDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get vendor detail vendors share aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<AggreratorLine> LoadVendorDetailVendorsShareAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false);

        #endregion
    }
}
