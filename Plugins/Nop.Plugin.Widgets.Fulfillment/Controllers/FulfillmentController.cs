using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Factories;
using Nop.Plugin.Widgets.Fulfillment.Models;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.Fulfillment.Controllers
{
    public class FulfillmentController : BaseAdminController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly ISettingService _settingService;
        protected readonly INotificationService _notificationService;
        protected readonly IThreePlService _threePlService;
        protected readonly IThreePlWarehouseService _threePlWarehouseService;
        protected readonly IOrderService _orderService;
        protected readonly Fulfillmen3PLtSettings _fulfillmen3PLtSettings;
        protected readonly WidgetSettings _widgetSettings;
        protected readonly ICurrencyService _currencyService;
        protected readonly CurrencySettings _currencySettings;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IFulfillmentModelFactory _fulfillmentModelFactory;

        #endregion

        #region Ctor

        public FulfillmentController(ILocalizationService localizationService,
            ISettingService settingService,
            INotificationService notificationService,
            IThreePlService threePlService,
            IThreePlWarehouseService threePlWarehouseService,
            IOrderService orderService,
            Fulfillmen3PLtSettings fulfillmen3PLtSettings,
            WidgetSettings widgetSettings,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IFulfillmentModelFactory fulfillmentModelFactory)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _notificationService = notificationService;
            _threePlService = threePlService;
            _threePlWarehouseService = threePlWarehouseService;
            _orderService = orderService;
            _fulfillmen3PLtSettings = fulfillmen3PLtSettings;
            _widgetSettings = widgetSettings;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _fulfillmentModelFactory = fulfillmentModelFactory;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel()
            {
                ThreePlEnable = await _threePlService.PluginActiveAsync(),
                ClientId = _fulfillmen3PLtSettings.ClientId,
                ClientSecret = _fulfillmen3PLtSettings.ClientSecret,
                ThreePlKey = _fulfillmen3PLtSettings.ThreePlKey,
                UserId = _fulfillmen3PLtSettings.UserId,
                CustomerId = _fulfillmen3PLtSettings.CustomerId,
                FacilityId = _fulfillmen3PLtSettings.FacilityId,
                SendOrderHoursInterval = _fulfillmen3PLtSettings.SendOrderHoursInterval,
                MultiPackageForCountries = _fulfillmen3PLtSettings.MultiPackageForCountries,
                MultiPackageOrderAmountOver = _fulfillmen3PLtSettings.MultiPackageOrderAmountOver,
                NoSplitAmountLess = _fulfillmen3PLtSettings.NoSplitAmountLess,
                ValidateCredentials = _fulfillmen3PLtSettings.ValidateCredentials()
            };

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries, false);

            return View("~/Plugins/Widgets.Fulfillment/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //set widget
            if (model.ThreePlEnable)
            {
                if (!_widgetSettings.ActiveWidgetSystemNames.Contains(FulfillmentDefaults.SystemName))
                {
                    _widgetSettings.ActiveWidgetSystemNames.Add(FulfillmentDefaults.SystemName);
                    await _settingService.SaveSettingAsync(_widgetSettings);
                }
            }
            else
            {
                if (_widgetSettings.ActiveWidgetSystemNames.Contains(FulfillmentDefaults.SystemName))
                {
                    _widgetSettings.ActiveWidgetSystemNames.Remove(FulfillmentDefaults.SystemName);
                    await _settingService.SaveSettingAsync(_widgetSettings);
                }
            }

            //set settings
            _fulfillmen3PLtSettings.ClientId = model.ClientId;
            _fulfillmen3PLtSettings.ClientSecret = model.ClientSecret;
            _fulfillmen3PLtSettings.ThreePlKey = model.ThreePlKey;
            _fulfillmen3PLtSettings.UserId = model.UserId;
            _fulfillmen3PLtSettings.CustomerId = model.CustomerId;
            _fulfillmen3PLtSettings.FacilityId = model.FacilityId;
            _fulfillmen3PLtSettings.SendOrderHoursInterval = model.SendOrderHoursInterval;
            _fulfillmen3PLtSettings.MultiPackageForCountries = model.MultiPackageForCountries.ToList();
            _fulfillmen3PLtSettings.MultiPackageOrderAmountOver = model.MultiPackageOrderAmountOver;
            _fulfillmen3PLtSettings.NoSplitAmountLess = model.NoSplitAmountLess;

            await _settingService.SaveSettingAsync(_fulfillmen3PLtSettings);

            if (_fulfillmen3PLtSettings.ValidateCredentials() && !_fulfillmen3PLtSettings.ValidateAccessToken())
            {
                var token = await _threePlWarehouseService.GetAuthenticationToken(_fulfillmen3PLtSettings.ClientId, _fulfillmen3PLtSettings.ClientSecret,
                    _fulfillmen3PLtSettings.ThreePlKey, _fulfillmen3PLtSettings.UserId);
                if (!string.IsNullOrEmpty(token))
                {
                    _fulfillmen3PLtSettings.AccessToken = token;
                    _fulfillmen3PLtSettings.ExpireOn = DateTime.UtcNow;

                    await _settingService.SaveSettingAsync(_fulfillmen3PLtSettings);
                }
            }

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region ThreePl logs

        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlRecords()
        {
            var searchModel = await _fulfillmentModelFactory.PrepareThreePlRecordsSearchModelAsync(new ThreePlRecordSearchModel());
            return View("~/Plugins/Widgets.Fulfillment/Views/ThreePlRecords.cshtml", searchModel);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlRecords(ThreePlRecordSearchModel searchModel)
        {
            //prepare model
            var model = await _fulfillmentModelFactory.PrepareThreePlRecordsListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlOrders(ThreePlOrderSearchModel searchModel)
        {
            //prepare model
            var model = await _fulfillmentModelFactory.PrepareThreePlOrdersListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> SyncThreePlRecord(int id)
        {
            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.Deleted)
                return Content(string.Empty);

            //check payment status
            if (order.PaymentStatus != PaymentStatus.Paid && !order.PaymentMethodSystemName.Equals("Payments.PurchaseOrder"))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Error.Payment"));
                return Content(string.Empty);
            }

            //check plugin was enable to sync data with 3PL
            if (await _threePlService.PluginActiveAsync())
            {
                //retrive access token
                var accessToken = _fulfillmen3PLtSettings.AccessToken;
                if (!_fulfillmen3PLtSettings.ValidateAccessToken())
                    accessToken = await _threePlWarehouseService.GetAuthenticationToken(_fulfillmen3PLtSettings.ClientId, _fulfillmen3PLtSettings.ClientSecret,
                        _fulfillmen3PLtSettings.ThreePlKey, _fulfillmen3PLtSettings.UserId);

                var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                if (threePlRecord != null)
                {
                    var syncOrders = await _threePlWarehouseService.SyncOrders(accessToken, new List<ThreePlRecord> { threePlRecord }, true);

                    if (syncOrders.Any())
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Success"));
                    else
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Error"));
                }
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> RequeueThreePlRecord(int id)
        {
            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.Deleted)
                return Content(string.Empty);

            //check plugin was enable to sync data with 3PL
            if (await _threePlService.PluginActiveAsync())
            {
                var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                if (threePlRecord != null)
                {
                    threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Pending;
                    await _threePlService.UpdateThreePlRecordAsync(threePlRecord);

                    //Check product warehouse change and update record
                    await _threePlWarehouseService.CheckProductWarehouseChange(order.Id);
                }
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> CancelThreePlRecord(int id)
        {
            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.Deleted)
                return Content(string.Empty);

            //check plugin was enable to sync data with 3PL
            if (await _threePlService.PluginActiveAsync())
            {
                var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                if (threePlRecord != null)
                {
                    threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Cancel;
                    threePlRecord.Note = "3PL Central - Admin forcefully cancelled with partially sync. Remain items not require to ship and not require to cancel partially sync order.";
                    await _threePlService.UpdateThreePlRecordAsync(threePlRecord);
                }
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> CancelThreePlOrder(int id)
        {
            //try to get an order with the specified id
            var plOrder = await _threePlService.GetThreePlOrderByIdAsync(id);
            if (plOrder == null || plOrder.CancelledDate.HasValue)
                return Content(string.Empty);

            //check plugin was enable to sync data with 3PL
            if (await _threePlService.PluginActiveAsync())
            {
                var threePlRecord = await _threePlService.GetThreePlRecordByIdAsync(plOrder.ThreePlId);
                if (threePlRecord != null)
                {
                    //update master record for pending to sync on child cancel order
                    threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Pending;
                    threePlRecord.Note = $"3PL Central - 3PL # {plOrder.ThreePlOrderId} Order cancelled. Require to sync.";
                    await _threePlService.UpdateThreePlRecordAsync(threePlRecord);

                    //update to sync cancel order
                    plOrder.ThreePlStutusId = (int)ThreePlStutus.Pending;
                    plOrder.CancelledDate = DateTime.UtcNow;
                    await _threePlService.UpdateThreePlOrderAsync(plOrder);
                }
            }

            return Json(new { success = true });
        }

        #endregion

        #region ThreePl shipping methods
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public IActionResult ThreePlShippingMethods()
        {
            //prepare model
            var searchModel = _fulfillmentModelFactory.PrepareThreePlShippingSearchModel(new ThreePlShippingMethodSearchModel());

            return View("~/Plugins/Widgets.Fulfillment/Views/ThreePlShippingMethods.cshtml", searchModel);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlShippingMethods(ThreePlShippingMethodSearchModel searchModel)
        {
            var model = await _fulfillmentModelFactory.PrepareThreePlShippingListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlShippingMethodUpdate([Validate] ThreePlShippingMethodModel model)
        {
            if (model.ShippingMethod != null)
                model.ShippingMethod = model.ShippingMethod.Trim();
            if (model.ThreePlCarrier != null)
                model.ThreePlCarrier = model.ThreePlCarrier.Trim();
            if (model.ThreePlService != null)
                model.ThreePlService = model.ThreePlService.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var threePlShippingMethod = await _threePlService.GetThreePlShippingMethodByIdAsync(model.Id);
            // if the ShippingMethod changed, ensure it isn't being used by another ShippingMethod
            if (!threePlShippingMethod.ShippingMethod.Equals(model.ShippingMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                var res = await _threePlService.GetThreePlShippingMethodByShippingMethodAsync(model.ShippingMethod);
                if (res != null && res.Id != threePlShippingMethod.Id)
                {
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.ShippingMethod.NameAlreadyExists"), res.ShippingMethod));
                }
            }

            //fill entity from model
            threePlShippingMethod.ShippingMethod = model.ShippingMethod;
            threePlShippingMethod.ThreePlCarrier = model.ThreePlCarrier;
            threePlShippingMethod.ThreePlService = model.ThreePlService;

            await _threePlService.UpdateThreePlShippingMethodAsync(threePlShippingMethod);

            return new NullJsonResult();
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlShippingMethodAdd([Validate] ThreePlShippingMethodModel model)
        {
            if (model.ShippingMethod != null)
                model.ShippingMethod = model.ShippingMethod.Trim();
            if (model.ThreePlCarrier != null)
                model.ThreePlCarrier = model.ThreePlCarrier.Trim();
            if (model.ThreePlService != null)
                model.ThreePlService = model.ThreePlService.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var res = await _threePlService.GetThreePlShippingMethodByShippingMethodAsync(model.ShippingMethod);
            if (res == null)
            {
                //fill entity from model
                var threePlShippingMethod = new ThreePlShippingMethod()
                {
                    ShippingMethod = model.ShippingMethod,
                    ThreePlCarrier = model.ThreePlCarrier,
                    ThreePlService = model.ThreePlService
                };

                await _threePlService.InsertThreePlShippingMethodAsync(threePlShippingMethod);
            }
            else
            {
                return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.ShippingMethod.NameAlreadyExists"), model.ShippingMethod));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> ThreePlShippingMethodDelete(int id)
        {
            //try to get a ShippingMethod with the specified id
            var threePlShippingMethod = await _threePlService.GetThreePlShippingMethodByIdAsync(id)
                ?? throw new ArgumentException("No 3PL shipping method found with the specified id", nameof(id));

            await _threePlService.DeleteThreePlShippingMethodAsync(threePlShippingMethod);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
