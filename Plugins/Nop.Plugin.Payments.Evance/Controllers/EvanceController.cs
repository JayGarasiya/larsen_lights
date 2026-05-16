using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Evance.BaseApiCall;
using Nop.Plugin.Payments.Evance.Domain;
using Nop.Plugin.Payments.Evance.Models;
using Nop.Plugin.Payments.Evance.Service;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.Evance.Contollers
{
    public class EvanceController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly EvanceSettings _evanceSettings;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWorkContext _workContext;
        private readonly ICustomerVaultService _customerVaultService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public EvanceController(IPermissionService permissionService,
            EvanceSettings settings,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWebHostEnvironment hostingEnvironment,
            IWorkContext workContext,
            ICustomerVaultService customerVaultService,
            ILogger logger,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService)
        {
            _permissionService = permissionService;
            _evanceSettings = settings;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _hostingEnvironment = hostingEnvironment;
            _workContext = workContext;
            _customerVaultService = customerVaultService;
            _logger = logger;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        #region Configuration

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Configure()
        {
            //whether user has the authority to manage configuration
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var evanceSettings = await _settingService.LoadSettingAsync<EvanceSettings>();

            var model = new ConfigurationModel
            {
                Enable = _evanceSettings.Enable,
                SecurityKey = _evanceSettings.SecurityKey,
                AdditionalFeePercentage = _evanceSettings.AdditionalFeePercentage,
                AdditionalFee = _evanceSettings.AdditionalFee,
                PaymentOptionIds = _evanceSettings.PaymentOptions?.ToList() ?? new List<int>(),
                CustomerVaultEnable = _evanceSettings.CustomerVaultEnabled,
                AllowSaveCard = _evanceSettings.AllowSaveCard,
                AllowSaveACH = _evanceSettings.AllowSaveACH,
                PaymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl,
                QueryAPIBaseUrl = _evanceSettings.QueryAPIBaseUrl,
                TransactionTypeId = (int)_evanceSettings.TransactionType,
                ThreedSEnable = _evanceSettings.ThreedSEnable,
                DisplaySavedDetails = _evanceSettings.DisplaySavedDetails,
                CollectCheckoutKey = _evanceSettings.CollectCheckoutKey,
                DropTable = _evanceSettings.DropTable,
                ExcludeCustomerRoleIds = _evanceSettings.ExcludeThreeDSToCustomerRoles?.ToList() ?? new List<int>(),
            };

            model.AvailablePaymentOptionIds = Enum.GetValues(typeof(PaymentOptions)).Cast<PaymentOptions>()
                .Select(payments => new SelectListItem
                {
                    Text = payments.ToString(),
                    Value = ((int)payments).ToString()
                }).ToList();

            model.AvailableTransactionTypesIds = Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>()
                .Select(transactionTypes => new SelectListItem
                {
                    Text = transactionTypes.ToString(),
                    Value = ((int)transactionTypes).ToString()
                }).ToList();

            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            model.AvailableExcludeCustomerRoleTds = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.ExcludeCustomerRoleIds.Contains(role.Id)
            }).ToList();

            model.Enable = _evanceSettings.Enable;
            model.SecurityKey = _evanceSettings.SecurityKey;
            model.AdditionalFee = _evanceSettings.AdditionalFee;
            model.ThreedSEnable = _evanceSettings.ThreedSEnable;
            model.CustomerVaultEnable = _evanceSettings.CustomerVaultEnabled;
            model.AllowSaveCard = _evanceSettings.AllowSaveCard;
            model.AllowSaveACH = _evanceSettings.AllowSaveACH;
            model.QueryAPIBaseUrl = _evanceSettings.QueryAPIBaseUrl;
            model.PaymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl;
            model.AdditionalFeePercentage = _evanceSettings.AdditionalFeePercentage;
            model.DisplaySavedDetails = _evanceSettings.DisplaySavedDetails;
            model.CollectCheckoutKey = _evanceSettings.CollectCheckoutKey;
            model.DropTable = _evanceSettings.DropTable;

            return View("~/Plugins/Payments.Evance/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //whether user has the authority to manage configuration
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            _evanceSettings.Enable = model.Enable;
            _evanceSettings.ThreedSEnable = model.ThreedSEnable;
            _evanceSettings.SecurityKey = model.SecurityKey;
            _evanceSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            _evanceSettings.AdditionalFee = model.AdditionalFee;
            _evanceSettings.PaymentOptions = model.PaymentOptionIds?.ToList() ?? new List<int>();
            _evanceSettings.ExcludeThreeDSToCustomerRoles = model.ExcludeCustomerRoleIds?.ToList() ?? new List<int>();
            _evanceSettings.CustomerVaultEnabled = model.CustomerVaultEnable;
            _evanceSettings.AllowSaveCard = model.AllowSaveCard;
            _evanceSettings.AllowSaveACH = model.AllowSaveACH;
            _evanceSettings.PaymentAPIBaseUrl = model.PaymentAPIBaseUrl;
            _evanceSettings.QueryAPIBaseUrl = model.QueryAPIBaseUrl;
            _evanceSettings.DisplaySavedDetails = model.DisplaySavedDetails;
            _evanceSettings.CollectCheckoutKey = model.CollectCheckoutKey;
            _evanceSettings.DropTable = model.DropTable;
            _evanceSettings.TransactionType = (TransactionType)model.TransactionTypeId;

            await _settingService.SaveSettingAsync(_evanceSettings);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region Verification file

        public IActionResult GetFile(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return Content(string.Empty);

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, ".well-known", folder);
            if (!System.IO.File.Exists(filePath))
                filePath = Path.Combine(_hostingEnvironment.ContentRootPath, ".well-known", folder);

            if (System.IO.File.Exists(filePath))
                return new PhysicalFileResult(filePath, "text/plain");

            return Content(string.Empty);
        }

        #endregion

        #region Custome Vault

        public async Task<IActionResult> CustomerVaultList()
        {
            if (!_evanceSettings.Enable || !_evanceSettings.DisplaySavedDetails)
                return Content(string.Empty);

            var model = new PaymentInfoModel();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerVaults = await _customerVaultService.GetCustomerVaultsByCustomerIdAsync(customer.Id);
            var securityKey = _evanceSettings.SecurityKey;
            var queryAPIBaseUrl = _evanceSettings.QueryAPIBaseUrl;
            foreach (var customerVault in customerVaults)
            {
                var requestModel = new BaseRequestModel();

                requestModel.AddParameter("security_key", securityKey);
                requestModel.AddParameter("customer_vault_id", customerVault.CustomerVaultId);

                // Call API (ensure URL-encoded request)
                var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: queryAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                var parsedDocument = XDocument.Parse(response);
                var customerCardNumber = parsedDocument.Descendants("cc_number").FirstOrDefault()?.Value;
                var accountType = parsedDocument.Descendants("account_type").FirstOrDefault()?.Value;
                var expirationDate = parsedDocument.Descendants("cc_exp").FirstOrDefault()?.Value;
                var accountNumber = parsedDocument.Descendants("check_account").FirstOrDefault()?.Value;
                var cardType = parsedDocument.Descendants("cc_type").FirstOrDefault()?.Value.Trim().Replace(" ", "");

                if (string.IsNullOrEmpty(accountType))
                {
                    if (!string.IsNullOrEmpty(expirationDate) && !string.IsNullOrEmpty(customerCardNumber) && !string.IsNullOrEmpty(cardType))
                    {
                        var formattedDate = expirationDate.Insert(2, "/");

                        model.CardInformation.Add(new CustomerCardInfo
                        {
                            CustomerCardNumber = customerCardNumber,
                            CustomerExpirationDate = formattedDate,
                            CardType = cardType,
                            CustomerVaultId = customerVault.CustomerVaultId,
                        });
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(accountNumber))
                    {
                        model.ACHInformation.Add(new CustomerACHInfo
                        {
                            CustomerAccountNumber = accountNumber,
                            CustomerVaultId = customerVault.CustomerVaultId,
                        });
                    }
                }

            }
            return View("~/Plugins/Payments.Evance/Views/CustomerVaultInfo.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomerVault(string vaultId)
        {
            if (!_evanceSettings.Enable || !_evanceSettings.DisplaySavedDetails)
                return Content(string.Empty);

            var securityKey = _evanceSettings.SecurityKey;
            var paymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl;
            var requestModel = new BaseRequestModel();

            requestModel.AddParameter("security_key", securityKey);
            requestModel.AddParameter("customer_vault_id", vaultId);
            requestModel.AddParameter("customer_vault", "delete_customer");

            // Call API (ensure URL-encoded request)
            var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: paymentAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

            if (isSuccess)
            {
                var customerVault = await _customerVaultService.GetCustomerVaultByIdAsync(vaultId);
                if (customerVault != null)
                    await _customerVaultService.DeleteCustomerVaultAsync(customerVault);

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        #endregion

        #region Webhook

        [HttpPost]
        public async Task<IActionResult> CheckStatus()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                var payload = JsonConvert.DeserializeObject<EvanceWebhookWrapper>(body);

                if (payload == null)
                    return BadRequest();

                if (payload?.EventBody != null)
                {
                    var transactionId = payload.EventBody.TransactionId;

                    if (string.IsNullOrEmpty(transactionId) && payload.EventBody.SettledTransactionIds.Count == 0)
                        return BadRequest();

                    if (payload.EventType == "transaction.check.status.settle")
                    {
                        var order = await _customerVaultService.GetOrderByTransactionIdAsync(transactionId);

                        if (order == null)
                        {
                            _logger.Information($"can not find order with transaction Id: {transactionId}.");
                            return BadRequest();
                        }

                        var orderNotes = await _orderService.GetOrderNotesByOrderIdAsync(orderId: order.Id, displayToCustomer: false);

                        if (payload.EventBody.Action != null && !string.IsNullOrEmpty(payload.EventBody.Action?.ActionType) && payload.EventBody.Action?.ActionType == "settle")
                        {
                            if (payload.EventBody.Action.Success == "1")
                            {
                                if (payload.EventBody.Action.ResponseCode == "100")
                                {
                                    if (payload.EventBody.TransactionType == "ck")
                                    {
                                        var amount = Convert.ToDecimal(payload.EventBody.Action.Amount);

                                        if (amount == order?.OrderTotal && _orderProcessingService.CanMarkOrderAsPaid(order))
                                        {
                                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                                            await _orderService.UpdateOrderAsync(order);

                                            return Ok();
                                        }
                                        else
                                        {
                                            var noteAlreadyExists = orderNotes.Any(x =>
                                                x.Note != null &&
                                                (x.Note.Contains("can not mark as paid because either order status is") ||
                                                 x.Note.Contains("can ot mark as paid because either order status is"))
                                            );

                                            if (!noteAlreadyExists)
                                            {
                                                await _orderService.InsertOrderNoteAsync(new OrderNote
                                                {
                                                    OrderId = order.Id,
                                                    Note = $"can not mark as paid because either order status is {order.OrderStatus} and payment status is {order.PaymentStatus} or amount is {amount}",
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow
                                                });
                                            }
                                            return BadRequest();
                                        }
                                    }
                                }
                                else
                                {
                                    if (!orderNotes.Any(x => x.Note != null && (x.Note.Contains("Response code"))))
                                    {
                                        await _orderService.InsertOrderNoteAsync(new OrderNote
                                        {
                                            OrderId = order.Id,
                                            Note = $"Response code: {payload.EventBody.Action.ResponseCode}",
                                            DisplayToCustomer = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                        await _orderService.UpdateOrderAsync(order);
                                    }
                                    return BadRequest();
                                }
                            }
                            else
                            {
                                if (!orderNotes.Any(x => x.Note != null && (x.Note.Contains("Action code"))))
                                {
                                    await _orderService.InsertOrderNoteAsync(new OrderNote
                                    {
                                        OrderId = order.Id,
                                        Note = $" Action code: {payload.EventBody.Action?.Success}",
                                        DisplayToCustomer = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                    await _orderService.UpdateOrderAsync(order);
                                }
                                return BadRequest();
                            }
                        }
                        else
                        {
                            if (!orderNotes.Any(x => x.Note != null && (x.Note.Contains("Action type"))))
                            {
                                await _orderService.InsertOrderNoteAsync(new OrderNote
                                {
                                    OrderId = order.Id,
                                    Note = $"Action type:{payload.EventBody.Action?.ActionType}",
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                await _orderService.UpdateOrderAsync(order);
                            }
                            return BadRequest();
                        }
                    }
                    else
                    {
                        if (payload.EventType == "settlement.batch.complete")
                        {
                            if (payload.EventBody.SettledTransactionIds.Count > 0)
                            {
                                foreach (var settledTransactionId in payload.EventBody.SettledTransactionIds)
                                {
                                    var settledOrder = await _customerVaultService.GetOrderByTransactionIdAsync(settledTransactionId);
                                    if (settledOrder != null)
                                    {
                                        var settledOrderNotes = await _orderService.GetOrderNotesByOrderIdAsync(orderId: settledOrder.Id, displayToCustomer: false);

                                        var attributes = await _genericAttributeService.GetAttributesForEntityAsync(settledOrder.Id, nameof(Order));

                                        var settleAttribute = attributes.FirstOrDefault(x =>
                                            x.Key == EvanceDefaults.IsTransactionSettleAttributeKey);

                                        if (settleAttribute != null)
                                        {
                                            await _genericAttributeService.DeleteAttributeAsync(settleAttribute);
                                        }

                                        if (!settledOrderNotes.Any(x => x.Note != null && (x.Note.Contains("Transaction is settled in evance gateway"))))
                                        {
                                            await _orderService.InsertOrderNoteAsync(new OrderNote
                                            {
                                                OrderId = settledOrder.Id,
                                                Note = $"Transaction is settled in evance gateway",
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });
                                            await _orderService.UpdateOrderAsync(settledOrder);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Evance ACH Webhook: " + ex.Message, ex);
                return StatusCode(500);
            }
        }

        #endregion

        #endregion
    }
}
