using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Models;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Widgets.Fulfillment.Factories
{
    /// <summary>
    /// Factory to prepare models related to fulfillment functionality.
    /// </summary>
    public partial class FulfillmentModelFactory : IFulfillmentModelFactory
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IThreePlService _threePlService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor
        public FulfillmentModelFactory(ILocalizationService localizationService,
            IThreePlService threePlService,
            IDateTimeHelper dateTimeHelper)
        {
            _localizationService = localizationService;
            _threePlService = threePlService;
            _dateTimeHelper = dateTimeHelper;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare three pl records search model
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual async Task<ThreePlRecordSearchModel> PrepareThreePlRecordsSearchModelAsync(ThreePlRecordSearchModel searchModel)
        {

            //prepare available 3PL statuses
            var availableStatusItems = await ThreePlStutus.Pending.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
                searchModel.AvailableStatus.Add(statusItem);

            if (availableStatusItems.Any())
                searchModel.AvailableStatus.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.ThreePlOrderSearchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare three pl records list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual async Task<ThreePlRecordListModel> PrepareThreePlRecordsListModelAsync(ThreePlRecordSearchModel searchModel)
        {
            //prepare model
            var tsIds = new List<int>();
            if (searchModel.SearchStatusId != 0)
                tsIds.Add(searchModel.SearchStatusId);

            var etsIds = new List<int>();
            if (!tsIds.Any())
                etsIds.Add((int)ThreePlStutus.NotRequire);

            //get 3PL logs
            var threePls = await _threePlService.SearchThreePlRecordsAsync(searchModel.SearchOrderId, searchModel.SearchThreePlOrderId, tsIds, etsIds,
                onlyCheckMoneyOrder: searchModel.SearchPaidByCheck, excludeDeletedOrder: true, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ThreePlRecordListModel().PrepareToGridAsync(searchModel, threePls, () =>
            {

                return threePls.SelectAwait(async log =>
                {
                    var threePlModel = new ThreePlRecordModel()
                    {
                        Id = log.Id,
                        OrderId = log.OrderId,
                        ThreePlStutusId = log.ThreePlStutusId,
                        PaidByCheck = log.PaidByCheck,
                        Note = log.Note,
                    };

                    threePlModel.ThreePlStutus = await _localizationService.GetLocalizedEnumAsync(log.ThreePlStutus);
                    threePlModel.CancelledDate = log.CancelledDate.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(log.CancelledDate.Value, DateTimeKind.Utc)).ToString() : string.Empty;

                    return threePlModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare three pl orders list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual async Task<ThreePlOrderListModel> PrepareThreePlOrdersListModelAsync(ThreePlOrderSearchModel searchModel)
        {
            searchModel.SetGridPageSize();

            //get 3PL logs
            var threePlOrders = await _threePlService.SearchThreePlOrdersAsync(searchModel.ThreePlId, searchModel.SearchOrderId, searchModel.SearchThreePlOrderId,
                excludeDeletedOrder: true, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ThreePlOrderListModel().PrepareToGridAsync(searchModel, threePlOrders, () =>
            {

                return threePlOrders.SelectAwait(async order =>
                {
                    var threePlModel = new ThreePlOrderModel()
                    {
                        Id = order.Id,
                        ThreePlId = order.ThreePlId,
                        ThreePlOrderId = order.ThreePlOrderId,
                        ThreePlStutusId = order.ThreePlStutusId
                    };

                    threePlModel.ThreePlStutus = await _localizationService.GetLocalizedEnumAsync(order.ThreePlStutus);
                    threePlModel.CreationDate = order.CreationDate.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(order.CreationDate.Value, DateTimeKind.Utc)).ToString() : string.Empty;
                    threePlModel.CancelledDate = order.CancelledDate.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(order.CancelledDate.Value, DateTimeKind.Utc)).ToString() : string.Empty;

                    return threePlModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare three pl shipping search model
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual ThreePlShippingMethodSearchModel PrepareThreePlShippingSearchModel(ThreePlShippingMethodSearchModel searchModel)
        {
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare three pl shipping list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        public virtual async Task<ThreePlShippingMethodListModel> PrepareThreePlShippingListModelAsync(ThreePlShippingMethodSearchModel searchModel)
        {
            //get 3PL shipping methods
            var threePlShippingMethods = await _threePlService.GetAllThreePlShippingMethodsAsync(string.Empty, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new ThreePlShippingMethodListModel().PrepareToGrid(searchModel, threePlShippingMethods, () =>
            {
                return threePlShippingMethods.Select(provider => new ThreePlShippingMethodModel()
                {
                    Id = provider.Id,
                    ShippingMethod = provider.ShippingMethod,
                    ThreePlCarrier = provider.ThreePlCarrier,
                    ThreePlService = provider.ThreePlService
                });
            });

            return model;
        }

        #endregion
    }
}
