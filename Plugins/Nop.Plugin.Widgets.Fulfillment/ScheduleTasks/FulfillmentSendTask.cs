using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.Fulfillment.ScheduleTasks
{
    /// <summary>
    /// Represents a schedule task to synchronize order
    /// </summary>
    public class FulfillmentSendTask : IScheduleTask
    {
        #region Fields

        protected readonly Fulfillmen3PLtSettings _fulfillmen3PLtSettings;
        protected readonly IThreePlService _threePlService;
        protected readonly IThreePlWarehouseService _threePlWarehouseService;

        #endregion

        #region Ctor

        public FulfillmentSendTask(Fulfillmen3PLtSettings fulfillmen3PLtSettings,
            IThreePlService threePlService,
            IThreePlWarehouseService threePlWarehouseService)
        {
            _fulfillmen3PLtSettings = fulfillmen3PLtSettings;
            _threePlService = threePlService;
            _threePlWarehouseService = threePlWarehouseService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            if (await _threePlService.PluginActiveAsync())
            {
                //retrive access token
                var accessToken = _fulfillmen3PLtSettings.AccessToken;
                if (!_fulfillmen3PLtSettings.ValidateAccessToken())
                    accessToken = await _threePlWarehouseService.GetAuthenticationToken(_fulfillmen3PLtSettings.ClientId, _fulfillmen3PLtSettings.ClientSecret, _fulfillmen3PLtSettings.ThreePlKey, _fulfillmen3PLtSettings.UserId);

                #region Sync cancel order

                var order3PLCancel = await _threePlService.SearchThreePlRecordsAsync(tsIds: new List<int> { (int)ThreePlStutus.Pending }, etsIds: new List<int> { (int)ThreePlStutus.NotRequire },
                    onlyCancelledOrder: true, cancelledDate: DateTime.UtcNow.AddHours(-3));

                await _threePlWarehouseService.SyncCancelOrders(accessToken, order3PLCancel.ToList());

                #endregion

                #region Sync paid orders to 3pl

                var order3PLs = await _threePlService.SearchThreePlRecordsAsync(tsIds: new List<int> { (int)ThreePlStutus.Pending, (int)ThreePlStutus.PendingStock }, etsIds: new List<int> { (int)ThreePlStutus.Sent, (int)ThreePlStutus.NotRequire }, excludeCancelledOrder: true);
                await _threePlWarehouseService.SyncOrders(accessToken, order3PLs.ToList());

                #endregion
            }
        }

        #endregion
    }
}
