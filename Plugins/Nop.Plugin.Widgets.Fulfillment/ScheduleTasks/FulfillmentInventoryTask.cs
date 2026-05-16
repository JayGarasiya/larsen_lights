using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.Fulfillment.ScheduleTasks
{
    /// <summary>
    /// Represents a schedule task to synchronize inventory
    /// </summary>
    public class FulfillmentInventoryTask : IScheduleTask
    {
        #region Fields

        protected readonly Fulfillmen3PLtSettings _fulfillmen3PLtSettings;
        protected readonly IThreePlService _threePlService;
        protected readonly IThreePlWarehouseService _threePlWarehouseService;
        protected readonly IProductService _productService;
        protected readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        protected readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public FulfillmentInventoryTask(Fulfillmen3PLtSettings fulfillmen3PLtSettings,
            IThreePlService threePlService,
            IThreePlWarehouseService threePlWarehouseService,
            IProductService productService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ILocalizationService localizationService)
        {
            _fulfillmen3PLtSettings = fulfillmen3PLtSettings;
            _threePlService = threePlService;
            _threePlWarehouseService = threePlWarehouseService;
            _productService = productService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private async Task UpdateInventoryAsync(string accessToken, string page)
        {
            var stockSummary = await _threePlWarehouseService.GetItemStockSummary(accessToken, page);
            foreach (var item in stockSummary.Summaries)
            {
                var product = await _productService.GetProductBySkuAsync(item.ItemIdentifier.Sku);
                if (product != null)
                {
                    //some previously used values
                    var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                    var previousStockQuantity = product.StockQuantity;

                    //In “Inventory” - make it so the “Minimum stock qty” gets deducted from the “Stock quantity”
                    product.StockQuantity = (int)item.Available - product.MinStockQuantity;
                    await _productService.UpdateProductAsync(product);

                    //back in stock notifications
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                        product.BackorderMode == BackorderMode.NoBackorders &&
                        product.AllowBackInStockSubscriptions &&
                        await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                        prevTotalStockQuantity <= 0 &&
                        product.Published &&
                        !product.Deleted)
                    {
                        //await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                    }

                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                }
            }
            if (stockSummary._links != null)
            {
                if (stockSummary._links.Next != null)
                    await UpdateInventoryAsync(accessToken, stockSummary._links.Next.Href);
            }
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

                #region Sync inventory

                await UpdateInventoryAsync(accessToken, "/inventory/stocksummaries");

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
