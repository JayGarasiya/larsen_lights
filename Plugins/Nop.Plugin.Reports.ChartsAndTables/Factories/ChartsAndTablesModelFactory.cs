using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Reports.ChartsAndTables.Domain;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using System.Globalization;

namespace Nop.Plugin.Reports.ChartsAndTables.Factories
{
    /// <summary>
    /// Represents the charts and tables factory implementation
    /// </summary>
    public partial class ChartsAndTablesModelFactory : IChartsAndTablesModelFactory
    {
        #region Fields

        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly ChartsAndTablesSettings _chartsAndTablesSettings;
        protected readonly IWorkContext _workContext;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductService _productService;
        protected readonly ICategoryService _categoryService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IVendorService _vendorService;
        protected readonly ICountryService _countryService;
        protected readonly IStateProvinceService _stateProvinceService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IOrderService _orderService;
        protected readonly IPaymentPluginManager _paymentPluginManager;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IPriceCalculationService _priceCalculationService;

        #endregion

        #region Ctor

        public ChartsAndTablesModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
             ChartsAndTablesSettings chartsAndTablesSettings,
             IWorkContext workContext,
             IChartsAndTablesServices chartsAndTablesServices,
             IDateTimeHelper dateTimeHelper,
             IPriceFormatter priceFormatter,
             IProductService productService,
             ICategoryService categoryService,
             IManufacturerService manufacturerService,
             IVendorService vendorService,
             ICountryService countryService,
             IStateProvinceService stateProvinceService,
             ILocalizationService localizationService,
             IOrderService orderService,
             IPaymentPluginManager paymentPluginManager,
             IProductAttributeService productAttributeService,
             IPriceCalculationService priceCalculationService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _chartsAndTablesSettings = chartsAndTablesSettings;
            _workContext = workContext;
            _chartsAndTablesServices = chartsAndTablesServices;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _productAttributeService = productAttributeService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare default item
        /// </summary>
        /// <param name="items">Available items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
        /// <param name="defaultItemValue">Default item value; defaults 0</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        /// <summary>
        /// Prepare available shipping method
        /// </summary>
        /// <param name="items">Payment status items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use default value of the default item text</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareShippingMethodsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available payment statuses
            var availableShippingMethod = (await _orderService.SearchOrdersAsync()).Select(p => p.ShippingMethod).Distinct();
            foreach (var shippingMethod in availableShippingMethod.Where(p => !string.IsNullOrEmpty(p)))
            {
                items.Add(new SelectListItem { Value = shippingMethod, Text = shippingMethod });
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText, string.Empty);
        }

        #endregion

        #region Methods

        #region Product

        #region Menu

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportMenuProductSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.ProductReport = _chartsAndTablesSettings.ProductReport;
            searchModel.ProductMenuProductReport = _chartsAndTablesSettings.ProductMenuProductReport;
            searchModel.ProductMenuProductAttributeReport = _chartsAndTablesSettings.ProductMenuProductAttributeReport && (await _productAttributeService.GetAllProductAttributesAsync()).Any();
            searchModel.ProductMenuCategoryReport = _chartsAndTablesSettings.ProductMenuCategoryReport;
            searchModel.ProductMenuManufacturerReport = _chartsAndTablesSettings.ProductMenuManufacturerReport;
            searchModel.ProductMenuVendorReport = _chartsAndTablesSettings.ProductMenuVendorReport;

            if (searchModel.ProductReport)
                searchModel.ProductReport = (searchModel.ProductMenuProductReport || searchModel.ProductMenuProductAttributeReport || searchModel.ProductMenuCategoryReport || searchModel.ProductMenuManufacturerReport || searchModel.ProductMenuVendorReport);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product menu product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductMenuProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadProductMenuProductDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _productService.GetProductByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product menu product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductMenuProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductMenuProductAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product menu product attribute list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product attribute list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductMenuProductAttributeListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadProductMenuProductAttributeDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = $"{(await _productService.GetProductByIdAsync(data.Id))?.Name} <br /> {data.Title}";

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product menu product attribute aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu product attribute aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductMenuProductAttributeAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductMenuProductAttributeAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product menu category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu category list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductMenuCategoryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadProductMenuCategoryDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _categoryService.GetCategoryByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product menu category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu category aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductMenuCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductMenuCategoryAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        /// <summary>
        /// Prepare product menu manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu manufacturer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductMenuManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadProductMenuManufacturerDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _manufacturerService.GetManufacturerByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product menu manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu manufacturer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductMenuManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductMenuManufacturerAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        /// <summary>
        /// Prepare product menu vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu vendor list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductMenuVendorListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadProductMenuVendorDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _vendorService.GetVendorByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product menu vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product menu vendor aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductMenuVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductMenuVendorAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        #endregion

        #region Detail 

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportDetailProductSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.ProductReport = _chartsAndTablesSettings.ProductReport;
            searchModel.ProductDetailProductReport = _chartsAndTablesSettings.ProductDetailProductReport;
            searchModel.ProductDetailProductAttributeReport = _chartsAndTablesSettings.ProductDetailProductAttributeReport && (await _productAttributeService
                .GetAllProductAttributeCombinationsAsync(searchModel.ProductId)).Any();
            searchModel.ProductDetailCustomerReport = _chartsAndTablesSettings.ProductDetailCustomerReport;
            searchModel.ProductDetailCountryReport = _chartsAndTablesSettings.ProductDetailCountryReport;
            searchModel.ProductDetailGlobalReport = _chartsAndTablesSettings.ProductDetailGlobalReport;

            if (searchModel.ProductReport)
                searchModel.ProductReport = (searchModel.ProductDetailProductReport || searchModel.ProductDetailProductAttributeReport || searchModel.ProductDetailCustomerReport || searchModel.ProductDetailCountryReport || searchModel.ProductDetailGlobalReport);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available billing countries
            searchModel.AvailableCountries = (await _countryService.GetAllCountriesForBillingAsync(showHidden: true))
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare available customer roles
            await _baseAdminModelFactory.PrepareCustomerRolesAsync(searchModel.AvailableRoles);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var productData = await _chartsAndTablesServices.LoadProductDetailProductDataAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductDetailProductAggreratorAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product detail product attribute list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product attribute list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductDetailProductAttributeListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var productData = await _chartsAndTablesServices.LoadProductDetailProductAttributeDataAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product detail product attribute aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail product attribute aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductDetailProductAttributeAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductDetailProductAttributeAggreratorAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product detail customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail customer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductDetailCustomerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var productData = await _chartsAndTablesServices.LoadProductDetailCustomerDataAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = data.Title.Equals("Guest") ? await _localizationService.GetResourceAsync("Admin.Customers.Guest") : data.Title;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product detail customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail customer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductDetailCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductDetailCustomerAggreratorAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product detail country list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail country list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductDetailCountryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var productData = await _chartsAndTablesServices.LoadProductDetailCountryDataAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    var country = await _countryService.GetCountryByIdAsync(data.Id);
                    var state = await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(data.Title));
                    result.Title = string.Join("", new[] { country?.Name, string.IsNullOrEmpty(state?.Name) ? string.Empty : $" >> {state?.Name}" });

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product detail country aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail country aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareProductDetailCountryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadProductDetailCountryAggreratorAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare product detail global list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product detail global list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareProductDetailGlobalListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var productData = await _chartsAndTablesServices.LoadProductDetailGlobalDataAsync(productId: searchModel.ProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ProductTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = data.Title switch
                        {
                            "Total Sales" => searchModel.IsChartReport ? (await _priceCalculationService.RoundPriceAsync(data.TotalAmount)).ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false),
                            "Total Quantity" => Math.Round(data.TotalAmount).ToString(CultureInfo.CurrentCulture),
                            "Quantity Order" => Math.Round(data.TotalAmount).ToString(CultureInfo.CurrentCulture),
                            _ => data.Title
                        },
                        Title = data.Title switch
                        {
                            "Total Sales" => await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Products.Detail.Global.Sales"),
                            "Total Quantity" => await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Products.Detail.Global.Quantity"),
                            "Quantity Order" => await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Products.Detail.Global.Orders"),
                            _ => data.Title
                        }
                    };

                    return result;
                });
            });

            return model;
        }

        #endregion

        #endregion

        #region Customer

        #region Menu

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportMenuCustomerSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.CustomerReport = _chartsAndTablesSettings.CustomerReport;
            if (searchModel.CustomerReport)
                searchModel.CustomerReport = (_chartsAndTablesSettings.CustomerMenuCustomerReport || _chartsAndTablesSettings.CustomerMenuRegisteredCustomerReport || _chartsAndTablesSettings.CustomerMenuGenderReport || _chartsAndTablesSettings.CustomerMenuCustomerRolesReport || _chartsAndTablesSettings.CustomerMenuCountryReport);

            searchModel.CustomerMenuCustomerReport = _chartsAndTablesSettings.CustomerMenuCustomerReport;
            searchModel.CustomerMenuRegisteredCustomerReport = _chartsAndTablesSettings.CustomerMenuRegisteredCustomerReport;
            searchModel.CustomerMenuGenderReport = _chartsAndTablesSettings.CustomerMenuGenderReport;
            searchModel.CustomerMenuCustomerRolesReport = _chartsAndTablesSettings.CustomerMenuCustomerRolesReport;
            searchModel.CustomerMenuCountryReport = _chartsAndTablesSettings.CustomerMenuCountryReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available billing countries
            searchModel.AvailableCountries = (await _countryService.GetAllCountriesForBillingAsync(showHidden: true))
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare available customer roles
            await _baseAdminModelFactory.PrepareCustomerRolesAsync(searchModel.AvailableRoles);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare customer menu customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerMenuCustomerDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = data.Title.Equals("Guest") ? await _localizationService.GetResourceAsync("Admin.Customers.Guest") : data.Title;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerMenuCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerMenuCustomerAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer registered list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer registered list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerRegisteredListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerMenuCustomerRegisteredDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer registered aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer registered aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerMenuCustomerRegisteredAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerMenuCustomerRegisteredAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer gender list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer gender list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerGenderListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerMenuCustomerGenderDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title switch
                        {
                            "M" => await _localizationService.GetResourceAsync("Account.Fields.Gender.Male"),
                            "F" => await _localizationService.GetResourceAsync("Account.Fields.Gender.Female"),
                            _ => data.Title
                        },
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer gender aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer gender aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerMenuCustomerGenderAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerMenuCustomerGenderAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer role list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer role list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCustomerRoleListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerMenuCustomerRoleDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer menu customer role aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu customer role aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerMenuCustomerRoleAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerMenuCustomerRoleAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer menu country list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu country list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerMenuCountryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerMenuCountryDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer menu country aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer menu country aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerMenuCountryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerMenuCountryAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                customerRoleId: searchModel.RoleId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        #endregion

        #region Detail

        /// <summary>
        /// Prepare chart and table search model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chart and table search model
        /// </returns>
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportDetailCustomerSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.CustomerReport = _chartsAndTablesSettings.CustomerReport;
            if (searchModel.CustomerReport)
                searchModel.CustomerReport = (_chartsAndTablesSettings.CustomerDetailCustomerReport || _chartsAndTablesSettings.CustomerDetailProductReport || _chartsAndTablesSettings.CustomerDetailCategoryReport || _chartsAndTablesSettings.CustomerDetailManufacturerReport || _chartsAndTablesSettings.CustomerDetailVendorReport);

            searchModel.CustomerDetailCustomerReport = _chartsAndTablesSettings.CustomerDetailCustomerReport;
            searchModel.CustomerDetailProductReport = _chartsAndTablesSettings.CustomerDetailProductReport;
            searchModel.CustomerDetailCategoryReport = _chartsAndTablesSettings.CustomerDetailCategoryReport;
            searchModel.CustomerDetailManufacturerReport = _chartsAndTablesSettings.CustomerDetailManufacturerReport;
            searchModel.CustomerDetailVendorReport = _chartsAndTablesSettings.CustomerDetailVendorReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare customer detail customer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail customer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerDetailCustomerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var customerData = await _chartsAndTablesServices.LoadCustomerDetailCustomerDataAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer detail customer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail customer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerDetailCustomerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerDetailCustomerAggreratorAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadCustomerDetailProductDataAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _productService.GetProductByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerDetailProductAggreratorAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail category list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var categoryData = await _chartsAndTablesServices.LoadCustomerDetailCategoryDataAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, categoryData, () =>
            {
                //fill in model values from the entity
                return categoryData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _categoryService.GetCategoryByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail category aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerDetailCategoryAggreratorAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail manufacturer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadCustomerDetailManufacturerDataAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _manufacturerService.GetManufacturerByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail manufacturer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerDetailManufacturerAggreratorAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare customer detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail vendor list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCustomerDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var vendorData = await _chartsAndTablesServices.LoadCustomerDetailVendorDataAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CustomerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, vendorData, () =>
            {
                //fill in model values from the entity
                return vendorData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _vendorService.GetVendorByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer detail vendor aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCustomerDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCustomerDetailVendorAggreratorAsync(customerId: searchModel.CustomerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        #endregion

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
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportMenuOrderSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.OrderReport = _chartsAndTablesSettings.OrderReport;
            if (searchModel.OrderReport)
                searchModel.OrderReport = (_chartsAndTablesSettings.OrderMenuOrderReport || _chartsAndTablesSettings.OrderMenuOrderItemReport || _chartsAndTablesSettings.OrderMenuShippingMethodReport || _chartsAndTablesSettings.OrderMenuPaymentMethodReport);

            searchModel.OrderMenuOrderReport = _chartsAndTablesSettings.OrderMenuOrderReport;
            searchModel.OrderMenuOrderItemReport = _chartsAndTablesSettings.OrderMenuOrderItemReport;
            searchModel.OrderMenuShippingMethodReport = _chartsAndTablesSettings.OrderMenuShippingMethodReport;
            searchModel.OrderMenuPaymentMethodReport = _chartsAndTablesSettings.OrderMenuPaymentMethodReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available billing countries
            searchModel.AvailableCountries = (await _countryService.GetAllCountriesForBillingAsync(showHidden: true))
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available payment methods
            searchModel.AvailablePaymentMethods = (await _paymentPluginManager.LoadAllPluginsAsync()).Select(method =>
                new SelectListItem { Text = method.PluginDescriptor.FriendlyName, Value = method.PluginDescriptor.SystemName }).ToList();
            searchModel.AvailablePaymentMethods.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = string.Empty });

            //prepare available shipping methods
            await PrepareShippingMethodsAsync(searchModel.AvailableShippingMethods);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare order menu order sales in time list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu order sales in time list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareOrderMenuOrderListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadOrderMenuOrderDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.OrderTopNRecord : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare order menu order sales in time aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu sales in time aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareOrderMenuOrderAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadOrderMenuOrderAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare order menu order item sales in time list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu order sales in time list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareOrderMenuOrderItemListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadOrderMenuOrderItemDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.OrderTopNRecord : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare order menu order item sales in time aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu sales in time aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareOrderMenuOrderItemAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadOrderMenuOrderItemAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare order menu order shipping method list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu shipping method list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareOrderMenuShippingMethodListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadOrderMenuShippingMethodDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.OrderTopNRecord : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare order menu shipping method aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu shipping method aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareOrderMenuShippingMethodAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadOrderMenuShippingMethodAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare order menu order payment method list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu payment method list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareOrderMenuPaymentMethodListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var customerData = await _chartsAndTablesServices.LoadOrderMenuPaymentMethodDataAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.OrderTopNRecord : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, customerData, () =>
            {
                //fill in model values from the entity
                return customerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare order menu payment method aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order menu payment method aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareOrderMenuPaymentMethodAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadOrderMenuPaymentMethodAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                countryId: searchModel.CountryId,
                vendorId: searchModel.VendorId,
                shippingMethod: searchModel.ShippingMethod,
                paymentMethod: searchModel.PaymentMethod,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

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
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportMenuTableSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.TableReport = _chartsAndTablesSettings.TableReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare table report list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the table report list model
        /// </returns>
        public virtual async Task<TableReportListModel> PrepareTableReportListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var tableData = await _chartsAndTablesServices.LoadTableReportAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new TableReportListModel().PrepareToGridAsync(searchModel, tableData, () =>
            {
                //fill in model values from the entity
                return tableData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new TableReportModel()
                    {
                        OrderId = data.OrderId,
                        CustomerId = data.CustomerId,
                        CustomerEmail = data.CustomerEmail,
                        ProductId = data.ProductId,
                        Quantity = data.Quantity,
                        UnitPriceExclTax = await _priceFormatter.FormatPriceAsync(data.UnitPriceExclTax, true, false),
                        LineTotalExclTax = await _priceFormatter.FormatPriceAsync(data.LineTotalExclTax, true, false),
                        PaymentMethod = data.PaymentMethod,
                        ShippingMethod = data.ShippingMethod,
                        BillingCountry = data.BillingCountry,
                        BillingCity = data.BillingCity,
                        YearMonth = data.YearMonth,
                    };


                    //fill in additional values (not existing in the entity)
                    var product = await _productService.GetProductByIdAsync(data.ProductId);
                    result.ProductName = string.Join("", new[] { product?.Name, string.IsNullOrEmpty(data.Attributes) ? string.Empty : $" <br /> {data.Attributes}" });
                    result.Sku = product?.Sku;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare table report aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the table report aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareTableReportAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadTableReportAggreratorAsync(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

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
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportDetailCategorySearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.CategoryReport = _chartsAndTablesSettings.CategoryReport;
            if (searchModel.CategoryReport)
                searchModel.CategoryReport = (_chartsAndTablesSettings.CategoryDetailCategoryReport || _chartsAndTablesSettings.CategoryDetailProductReport || _chartsAndTablesSettings.CategoryDetailShareCategoryReport || _chartsAndTablesSettings.CategoryDetailManufacturerReport || _chartsAndTablesSettings.CategoryDetailVendorReport);

            searchModel.CategoryDetailCategoryReport = _chartsAndTablesSettings.CategoryDetailCategoryReport;
            searchModel.CategoryDetailProductReport = _chartsAndTablesSettings.CategoryDetailProductReport;
            searchModel.CategoryDetailManufacturerReport = _chartsAndTablesSettings.CategoryDetailManufacturerReport;
            searchModel.CategoryDetailVendorReport = _chartsAndTablesSettings.CategoryDetailVendorReport;
            searchModel.CategoryDetailShareCategoryReport = _chartsAndTablesSettings.CategoryDetailShareCategoryReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare category detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail category list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCategoryDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var categoryData = await _chartsAndTablesServices.LoadCategoryDetailCategoryDataAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CategoryTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, categoryData, () =>
            {
                //fill in model values from the entity
                return categoryData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail category aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCategoryDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCategoryDetailCategoryAggreratorAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        /// <summary>
        /// Prepare category detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCategoryDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadCategoryDetailProductDataAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CategoryTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _productService.GetProductByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCategoryDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCategoryDetailProductAggreratorAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare category detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail manufacturer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCategoryDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadCategoryDetailManufacturerDataAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CategoryTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _manufacturerService.GetManufacturerByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail manufacturer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCategoryDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCategoryDetailManufacturerAggreratorAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare category detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail vendor list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCategoryDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var vendorData = await _chartsAndTablesServices.LoadCategoryDetailVendorDataAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CategoryTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, vendorData, () =>
            {
                //fill in model values from the entity
                return vendorData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _vendorService.GetVendorByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail vendor aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCategoryDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCategoryDetailVendorAggreratorAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare category detail categories share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail categories share list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareCategoryDetailCategoriesShareListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var categoryData = await _chartsAndTablesServices.LoadCategoryDetailCategoriesShareDataAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.CategoryTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, categoryData, () =>
            {
                //fill in model values from the entity
                return categoryData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _categoryService.GetCategoryByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category detail categories share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category detail categories share aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareCategoryDetailCategoriesShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadCategoryDetailCategoriesShareAggreratorAsync(categoryId: searchModel.CategoryId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                manufacturerId: searchModel.ManufacturerId,
                vendorId: searchModel.VendorId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

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
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportDetailManufacturerSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.ManufacturerReport = _chartsAndTablesSettings.ManufacturerReport;
            if (searchModel.ManufacturerReport)
                searchModel.ManufacturerReport = (_chartsAndTablesSettings.ManufacturerDetailManufacturerReport || _chartsAndTablesSettings.ManufacturerDetailProductReport || _chartsAndTablesSettings.ManufacturerDetailCategoryReport || _chartsAndTablesSettings.ManufacturerDetailShareManufacturerReport);

            searchModel.ManufacturerDetailManufacturerReport = _chartsAndTablesSettings.ManufacturerDetailManufacturerReport;
            searchModel.ManufacturerDetailProductReport = _chartsAndTablesSettings.ManufacturerDetailProductReport;
            searchModel.ManufacturerDetailCategoryReport = _chartsAndTablesSettings.ManufacturerDetailCategoryReport;
            searchModel.ManufacturerDetailShareManufacturerReport = _chartsAndTablesSettings.ManufacturerDetailShareManufacturerReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare manufacturer detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadManufacturerDetailManufacturerDataAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ManufacturerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareManufacturerDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadManufacturerDetailManufacturerAggreratorAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadManufacturerDetailProductDataAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ManufacturerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _productService.GetProductByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareManufacturerDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadManufacturerDetailProductAggreratorAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail category list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadManufacturerDetailCategoryDataAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ManufacturerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _categoryService.GetCategoryByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail category aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareManufacturerDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadManufacturerDetailCategoryAggreratorAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail manufacturers share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturers share list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareManufacturerDetailManufacturersShareListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadManufacturerDetailManufacturersShareDataAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.ManufacturerTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _manufacturerService.GetManufacturerByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer detail manufacturers share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer detail manufacturers share aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareManufacturerDetailManufacturersShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadManufacturerDetailManufacturersShareAggreratorAsync(manufacturerId: searchModel.ManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

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
        public virtual async Task<ChartsAndTablesSearchModel> PrepareReportDetailVendorSearchModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            //prepare start date and end date
            var currentDate = DateTime.Now;
            searchModel.StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            searchModel.EndDate = currentDate;

            //default settings 
            searchModel.VendorReport = _chartsAndTablesSettings.VendorReport;
            if (searchModel.VendorReport)
                searchModel.VendorReport = (_chartsAndTablesSettings.VendorDetailVendorReport || _chartsAndTablesSettings.VendorDetailProductReport || _chartsAndTablesSettings.VendorDetailCategoryReport || _chartsAndTablesSettings.VendorDetailManufacturerReport || _chartsAndTablesSettings.VendorDetailShareVendorReport);

            searchModel.VendorDetailVendorReport = _chartsAndTablesSettings.VendorDetailVendorReport;
            searchModel.VendorDetailProductReport = _chartsAndTablesSettings.VendorDetailProductReport;
            searchModel.VendorDetailCategoryReport = _chartsAndTablesSettings.VendorDetailCategoryReport;
            searchModel.VendorDetailManufacturerReport = _chartsAndTablesSettings.VendorDetailManufacturerReport;
            searchModel.VendorDetailShareVendorReport = _chartsAndTablesSettings.VendorDetailShareVendorReport;

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available order statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);

            //prepare available payment statuses
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare "group by" filter
            searchModel.GroupByOptions = (await ReportGroupByOptions.Month.ToSelectListAsync()).ToList();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare vendor detail vendor list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendor list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareVendorDetailVendorListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var vendorData = await _chartsAndTablesServices.LoadVendorDetailVendorDataAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.VendorTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, vendorData, () =>
            {
                //fill in model values from the entity
                return vendorData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Title = data.Title,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor detail vendor aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendor aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareVendorDetailVendorAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadVendorDetailVendorAggreratorAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                groupBy: (ReportGroupByOptions)searchModel.SearchGroupId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        /// <summary>
        /// Prepare vendor detail product list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail product list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareVendorDetailProductListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var productData = await _chartsAndTablesServices.LoadVendorDetailProductDataAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.VendorTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, productData, () =>
            {
                //fill in model values from the entity
                return productData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _productService.GetProductByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor detail product aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail product aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareVendorDetailProductAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadVendorDetailProductAggreratorAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare vendor detail category list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail category list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareVendorDetailCategoryListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var categoryData = await _chartsAndTablesServices.LoadVendorDetailCategoryDataAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.VendorTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, categoryData, () =>
            {
                //fill in model values from the entity
                return categoryData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _categoryService.GetCategoryByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor detail category aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail category aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareVendorDetailCategoryAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadVendorDetailCategoryAggreratorAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare vendor detail manufacturer list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail manufacturer list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareVendorDetailManufacturerListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var manufacturerData = await _chartsAndTablesServices.LoadVendorDetailManufacturerDataAsync(vendorId: searchModel.VendorId, 
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.VendorTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, manufacturerData, () =>
            {
                //fill in model values from the entity
                return manufacturerData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        Quantity = data.TotalQuantity,
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _manufacturerService.GetManufacturerByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor detail manufacturer aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail manufacturer aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareVendorDetailManufacturerAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadVendorDetailManufacturerAggreratorAsync(vendorId: searchModel.VendorId, 
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false),
                TotalQuantity = aggreratorData.TotalQuantity,
                AverageQuantity = aggreratorData.AverageQuantity,
                MaxQuantity = aggreratorData.MaxQuantity,
                MinQuantity = aggreratorData.MinQuantity
            };

            return model;
        }

        /// <summary>
        /// Prepare vendor detail vendors share list model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendors share list model
        /// </returns>
        public virtual async Task<ChartsAndTablesDataListModel> PrepareVendorDetailVendorsShareListModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var vendorData = await _chartsAndTablesServices.LoadVendorDetailVendorsShareDataAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.IsChartReport ? _chartsAndTablesSettings.VendorTopNChart : searchModel.PageSize);

            //prepare list model
            var model = await new ChartsAndTablesDataListModel().PrepareToGridAsync(searchModel, vendorData, () =>
            {
                //fill in model values from the entity
                return vendorData.SelectAwait(async data =>
                {
                    //fill in model values from the entity
                    var result = new ChartsAndTablesDataModel()
                    {
                        TotalSales = searchModel.IsChartReport ? data.TotalAmount.ToString(CultureInfo.CurrentCulture) : await _priceFormatter.FormatPriceAsync(data.TotalAmount, true, false)
                    };

                    //fill in additional values (not existing in the entity)
                    result.Title = (await _vendorService.GetVendorByIdAsync(data.Id))?.Name;

                    return result;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare vendor detail vendors share aggregator model
        /// </summary>
        /// <param name="searchModel">Chart and table search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor detail vendors share aggregator model
        /// </returns>
        public virtual async Task<AggreratorModel> PrepareVendorDetailVendorsShareAggregatorModelAsync(ChartsAndTablesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //get data
            var aggreratorData = await _chartsAndTablesServices.LoadVendorDetailVendorsShareAggreratorAsync(vendorId: searchModel.VendorId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                storeId: searchModel.StoreId,
                orderStatusId: searchModel.OrderStatusId,
                paymentStatusId: searchModel.PaymentStatusId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                priceWithTax: _chartsAndTablesSettings.PriceWithTax);

            var model = new AggreratorModel
            {
                TotalSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.TotalSalesValue, true, false),
                AverageSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.AverageSalesValue, true, false),
                MaxSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MaxSalesValue, true, false),
                MinSalesValue = await _priceFormatter.FormatPriceAsync(aggreratorData.MinSalesValue, true, false)
            };

            return model;
        }

        #endregion

        #endregion
    }
}
