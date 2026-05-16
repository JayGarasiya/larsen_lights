using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Misc.ShipmentTracking.Domain;
using Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping.Tracking;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using static Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking.UPSTrackingResponse.TrackResponse.Shipments;
using static Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking.UPSTrackingResponse.TrackResponse.Shipments.PackageInfo;

namespace Nop.Plugin.Misc.ShipmentTracking.Services
{
    /// <summary>
    /// Represents a shipment tracking service 
    /// </summary>
    public class ShipmentTrackingService : IShipmentTrackingService
    {
        #region Constant 

        private const string ShippingMethod = "3PLCarrier";
        private const string FedExAuthUrl = "/oauth/token";
        private const string FedExTrackingUrl = "/track/v1/trackingnumbers";

        #endregion

        #region Fields

        protected readonly ILogger _logger;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ShipmentTrackingSettings _shipmentTrackingSettings;
        protected readonly IOrderProcessingService _orderProcessingService;
        protected readonly AdminAreaSettings _adminAreaSettings;
        protected readonly IRepository<Address> _addressRepository;
        protected readonly IRepository<Order> _orderRepository;
        protected readonly IRepository<OrderItem> _orderItemRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<Shipment> _shipmentRepository;
        protected readonly IRepository<ShipmentItem> _siRepository;
        protected readonly IRepository<GenericAttribute> _genericAttributeRepository;
        protected readonly ILocalizationService _localizationService;
        protected readonly IRepository<OrderNote> _orderNoteRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly ISettingService _settingService;
        protected readonly IRepository<Customer> _customerRepository; 

        #endregion

        #region Ctor

        public ShipmentTrackingService(ILogger logger,
            IGenericAttributeService genericAttributeService,
            ShipmentTrackingSettings shipmentTrackingSettings,
            IOrderProcessingService orderProcessingService,
            AdminAreaSettings adminAreaSettings,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<ShipmentItem> siRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            ILocalizationService localizationService,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            ISettingService settingService,
            IRepository<Customer> customerRepsitory)
        {
            _logger = logger;
            _genericAttributeService = genericAttributeService;
            _shipmentTrackingSettings = shipmentTrackingSettings;
            _orderProcessingService = orderProcessingService;
            _adminAreaSettings = adminAreaSettings;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _shipmentRepository = shipmentRepository;
            _siRepository = siRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _localizationService = localizationService;
            _orderNoteRepository = orderNoteRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _settingService = settingService;
            _customerRepository = customerRepsitory;
        }

        #endregion

        #region Utilites 

        /// <summary>
        /// Get OAuth access token for UPS tracking API
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the UPS OAuth access token
        /// </returns>
        private async Task<string> GetUPSOAuthTokenAsync()
        {
            var clientId = _shipmentTrackingSettings.UPSClientId;
            var clientSecret = _shipmentTrackingSettings.UPSClientSecret;
            var authUrl = _shipmentTrackingSettings.UPSAuthApiUrl;

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.PostAsync(authUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<UPSOAuthResponse>(responseString);
                    return tokenResponse?.access_token;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await _logger.ErrorAsync($"UPS Auth Error: {error}");
                }
            }

            return null;
        }

        /// <summary>
        /// Check whether a UPS shipment is delivered using UPS tracking API
        /// </summary>
        /// <param name="shipment">Shipment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a tuple indicating success status and UPS tracking response
        /// </returns>
        private async Task<(bool isSuccess, UPSTrackingResponse Response)> UPSShipmentDeliveredAsync(Shipment shipment)
        {
            if (!_shipmentTrackingSettings.ValidateUPSCredentials())
            {
                await _logger.ErrorAsync("Shipment Tracking: Required settings for UPS tracking API are not configured");
                return (false, null);
            }

            try
            {
                if (!shipment.TrackingNumber.StartsWith("sp", StringComparison.OrdinalIgnoreCase))
                {
                    var token = string.Empty;
                    var savedToken = _shipmentTrackingSettings.UPSAuthToken;
                    var tokenGeneratedTime = _shipmentTrackingSettings.UPSAuthTokenGenerated;

                    // If token is empty or 3+ hours have passed since token was generated
                    if ((DateTime.Now - tokenGeneratedTime).TotalHours >= 3)
                    {
                        // Get new OAuth token
                        token = await GetUPSOAuthTokenAsync();
                        _shipmentTrackingSettings.UPSAuthToken = token;
                        _shipmentTrackingSettings.UPSAuthTokenGenerated = DateTime.Now;

                        await _settingService.SaveSettingAsync(_shipmentTrackingSettings);
                    }
                    else
                    {
                        token = savedToken;
                    }

                    if (string.IsNullOrEmpty(token))
                        return (false, null);

                    var requestJson = JsonConvert.SerializeObject(new
                    {
                        trackingNumber = new[] { shipment.TrackingNumber }
                    });

                    //call Tracking API
                    var trackingNumber = shipment.TrackingNumber;
                    var apiBaseUrl = _shipmentTrackingSettings.UPSTrackingApiUrl;
                    var apiUrl = $"{apiBaseUrl}{trackingNumber}";

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("transId", shipment.OrderId.ToString());
                        client.DefaultRequestHeaders.Add("transactionSrc", "Larsenlights");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await client.GetAsync(apiUrl);
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(json))
                        {
                            var upsResponse = JsonConvert.DeserializeObject<UPSTrackingResponse>(json);
                            if (!(upsResponse?.Response?.Shipment?.FirstOrDefault().Package.GetType() == typeof(JArray)))
                                upsResponse?.Response?.Shipment?.FirstOrDefault().Packages.Add(JsonConvert.DeserializeObject<PackageInfo>(upsResponse?.Response?.Shipment?.FirstOrDefault().Package.ToString()));
                            else
                                upsResponse?.Response?.Shipment?.FirstOrDefault().Packages.AddRange(JsonConvert.DeserializeObject<List<PackageInfo>>(upsResponse?.Response?.Shipment?.FirstOrDefault().Package.ToString()));

                            return (true, upsResponse);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Shipment Tracking (UPS) : {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
                return (false, null);
            }

            return (false, null);
        }

        /// <summary>
        /// Create USPS tracking request XML
        /// </summary>
        /// <param name="trackingNumber">Tracking number</param>
        /// <returns>
        /// XML request string for USPS tracking API
        /// </returns>
        private string CreateUSPSTrackRequest(string trackingNumber)
        {
            var document = new XDocument(
                new XElement("TrackFieldRequest", new XAttribute("USERID", _shipmentTrackingSettings.USPSUsername), new XAttribute("PASSWORD", _shipmentTrackingSettings.USPSPassword),
                    new XElement("TrackID", new XAttribute("ID", trackingNumber)))
            );

            return document.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Check whether a USPS shipment is delivered using USPS tracking API
        /// </summary>
        /// <param name="shipment">Shipment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains USPS tracking information
        /// </returns>
        private async Task<USPSTrackInfo> USPSShipmentDeliveredAsync(Shipment shipment)
        {
            if (!_shipmentTrackingSettings.ValidateUSPSCredentials())
                await _logger.ErrorAsync("Shipment Tracking: Required settings for USPS tracking API are not configured");

            try
            {
                //create request details
                var requestString = CreateUSPSTrackRequest(shipment.TrackingNumber);

                //configure client
                var client = new HttpClient();
                client.BaseAddress = new Uri($"{_shipmentTrackingSettings.USPSTrackingApiUrl}/ShippingAPI.dll");
                client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CURRENT_VERSION}");
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationXml);

                // Create a request for the URL.
                var stream = await client.GetStreamAsync($"?API=TrackV2&XML={requestString}");

                // Get the stream containing content returned by the server.
                return await USPSTrackInfo.LoadAsync(stream);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Shipment Tracking (USPS): {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
            }

            return null;
        }

        /// <summary>
        /// Create OAuth authorization token for FedEx tracking API
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains FedEx OAuth response
        /// </returns>
        private async Task<FedExAuthResponse> CreateFedExAuthMethodAsync()
        {
            if (!_shipmentTrackingSettings.ValidateFedExCredentials())
                await _logger.ErrorAsync("Shipment Tracking : Required settings for FedEx tracking API details are not configured");

            try
            {
                //create request details
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_shipmentTrackingSettings.FedExTrackingApiUrl);
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _shipmentTrackingSettings.FedExClientId),
                        new KeyValuePair<string, string>("client_secret", _shipmentTrackingSettings.FedExClientSecret)
                    });
                    var result = await client.PostAsync(FedExAuthUrl, content);
                    var response = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FedExAuthResponse>(response);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Shipment Tracking (FedEx OAuth): {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Prepare FedEx tracking request model
        /// </summary>
        /// <param name="trackingNumber">Tracking number</param>
        /// <param name="detailScans">Whether to include detailed scans</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains FedEx tracking request
        /// </returns>
        private Task<FedExTrackingRequest> CreateFedExTrackingRequest(string trackingNumber, bool detailScans = false)
        {
            var fedExTrackRequest = new FedExTrackingRequest()
            {
                DetailedScans = detailScans,
                Request = new List<FedExTrackingRequest.TrackingInfo>()
                {
                    {
                        new FedExTrackingRequest.TrackingInfo()
                        {
                            TrackNumberDetail = new FedExTrackingRequest.TrackingInfo.NumberInfo()
                            {
                                TrackingNumber = trackingNumber
                            }
                        }
                    }
                }
            };

            return Task.FromResult(fedExTrackRequest);
        }

        /// <summary>
        /// Track FedEx shipment using FedEx tracking API
        /// </summary>
        /// <param name="shipment">Shipment entity</param>
        /// <param name="detailScans">Whether to include detailed scans</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains FedEx tracking response
        /// </returns>
        private async Task<FedExTrackingResponse> FedExShipmentTrackingAsync(Shipment shipment, bool detailScans = false)
        {
            if (!_shipmentTrackingSettings.ValidateFedExCredentials())
                await _logger.ErrorAsync("Shipment Tracking: Required settings for FedEx tracking API details are not configured");

            //authorize token
            var tokenResponse = await CreateFedExAuthMethodAsync();
            if (tokenResponse != null)
            {
                if (tokenResponse.Errors?.Any() ?? false)
                {
                    await _logger.ErrorAsync($"Shipment Tracking (FedEx OAuth): {string.Join(", ", tokenResponse.Errors.Select(p => $"{p.Code} - {p.Message}"))} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                    return null;
                }

                try
                {
                    //create request details
                    var requestData = await CreateFedExTrackingRequest(shipment.TrackingNumber, detailScans);
                    var data = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(data, Encoding.UTF8, "application/json");

                    //create request details
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri($"{_shipmentTrackingSettings.FedExTrackingApiUrl}{FedExTrackingUrl}"),
                        Headers = {
                            { HeaderNames.Authorization, $"{tokenResponse.TokenType} {tokenResponse.Token}" },
                            { "X-locale", "en_US" },
                        },
                        Content = content
                    };

                    using (var response = await client.SendAsync(request))
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<FedExTrackingResponse>(body);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Shipment Tracking (FedEx): {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Check whether a Spee-Dee shipment is delivered
        /// </summary>
        /// <param name="shipment">Shipment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains Spee-Dee tracking response
        /// </returns>
        private async Task<SpeedeeRespone> SPEEDEEShipmentDeliveredAsync(Shipment shipment)
        {
            if (!_shipmentTrackingSettings.ValidateSpeedeeCredentials())
                await _logger.ErrorAsync("Shipment Tracking: Required settings for Speedee tracking API are not configured");

            try
            {
                // Create a request for the URL.
                var client = WebRequest.Create($"{_shipmentTrackingSettings.SpeedeeTrackingApiUrl}?ep=PackageTracking/{shipment.TrackingNumber}&acct={_shipmentTrackingSettings.SpeedeeAccount}&passwd={_shipmentTrackingSettings.SpeedeePassword}");

                // If required by the server, set the credentials.
                client.Credentials = CredentialCache.DefaultCredentials;

                // Get the response.
                var response = client.GetResponse();

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (var stream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    var reader = new StreamReader(stream);
                    var jsonString = reader.ReadToEnd();

                    return JsonConvert.DeserializeObject<SpeedeeRespone>(jsonString);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.UnknownError && !ex.Message.Contains("(503)"))
                    await _logger.ErrorAsync($"Shipment Tracking (Spee-Dee Delivery): {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("(503)"))
                    await _logger.ErrorAsync($"Shipment Tracking (Spee-Dee Delivery): {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
            }

            return null;
        }

        /// <summary>
        /// Check whether a DHL shipment is delivered using DHL tracking API
        /// </summary>
        /// <param name="shipment">Shipment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains DHL tracking response
        /// </returns>
        private async Task<DHLRespone> DHLShipmentDeliveredAsync(Shipment shipment)
        {
            if (!_shipmentTrackingSettings.ValidateDHLCredentials())
                await _logger.ErrorAsync("Shipment Tracking: Required settings for DHL tracking API are not configured");

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_shipmentTrackingSettings.DHLTrackingApiUrl}?trackingNumber={shipment.TrackingNumber}"),
                    Headers = { { "DHL-API-Key", _shipmentTrackingSettings.DHLConsumerKey }, },
                };

                using (var response = await client.SendAsync(request))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DHLRespone>(body);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Shipment Tracking (DHL): {ex.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}", ex);
            }

            return null;
        }

        /// <summary>
        /// Prepare shipment status event by the passed track activity
        /// </summary>
        /// <param name="activity">Track activity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipment status event 
        /// </returns>
        private async Task<ShipmentStatusEvent> PrepareShipmentStatusEventAsync(ActivityLogs activity)
        {
            var shipmentStatusEvent = new ShipmentStatusEvent();

            try
            {
                //prepare date
                shipmentStatusEvent.Date = DateTime
                    .ParseExact($"{activity.Date} {activity.Time}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

                //prepare address
                var addressDetails = new List<string>();
                if (!string.IsNullOrEmpty(activity.ActivityLocation?.Address?.CountryCode))
                    addressDetails.Add(activity.ActivityLocation.Address.CountryCode);
                if (!string.IsNullOrEmpty(activity.ActivityLocation?.Address?.StateProvinceCode))
                    addressDetails.Add(activity.ActivityLocation.Address.StateProvinceCode);
                if (!string.IsNullOrEmpty(activity.ActivityLocation?.Address?.City))
                    addressDetails.Add(activity.ActivityLocation.Address.City);
                if (activity.ActivityLocation?.Address?.AddressLine?.Any() ?? false)
                    addressDetails.Add(activity.ActivityLocation.Address.AddressLine);
                if (!string.IsNullOrEmpty(activity.ActivityLocation?.Address?.PostalCode))
                    addressDetails.Add(activity.ActivityLocation.Address.PostalCode);

                shipmentStatusEvent.CountryCode = activity.ActivityLocation?.Address?.CountryCode;
                shipmentStatusEvent.Location = string.Join(", ", addressDetails);

                if (activity.Status == null)
                    return shipmentStatusEvent;

                //prepare description
                var eventName = string.Empty;
                switch (activity.Status.Type)
                {
                    case "I":
                        eventName = activity.Status.Code switch
                        {
                            "DP" => "Plugins.Shipping.Tracker.Departed",
                            "EP" => "Plugins.Shipping.Tracker.ExportScanned",
                            "OR" => "Plugins.Shipping.Tracker.OriginScanned",
                            _ => "Plugins.Shipping.Tracker.Arrived",
                        };
                        break;

                    case "X":
                        eventName = "Plugins.Shipping.Tracker.NotDelivered";
                        break;

                    case "M":
                        eventName = "Plugins.Shipping.Tracker.Booked";
                        break;

                    case "D":
                        eventName = "Plugins.Shipping.Tracker.Delivered";
                        break;

                    case "P":
                        eventName = "Plugins.Shipping.Tracker.Pickup";
                        break;
                }
                shipmentStatusEvent.EventName = await _localizationService.GetResourceAsync(eventName);
            }
            catch
            {
                // ignored
            }

            return shipmentStatusEvent;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Track shipment
        /// </summary>
        public virtual async Task TrackShipmentAsync()
        {
            var shipments = await GetAllNotDeliveredShipmentsAsync(pageIndex: _shipmentTrackingSettings.CurrentPageIndex, pageSize: _adminAreaSettings.DefaultGridPageSize);

            foreach (var shipment in shipments)
            {
                var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
                if (!string.IsNullOrEmpty(shippingMethod) && !string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    if (shippingMethod.Equals(ShipmentMethod.UPS.ToString()))
                    {
                        var (isSuccess, response) = await UPSShipmentDeliveredAsync(shipment);

                        if (isSuccess && response != null)
                        {
                            var trackResponse = response.Response;
                            if (trackResponse != null &&
                                trackResponse?.Shipment != null && (trackResponse?.Shipment?.FirstOrDefault().Packages.Any() ?? false))
                            {
                                var package = trackResponse?.Shipment?.FirstOrDefault().Packages?.FirstOrDefault()?.Activity?.OrderByDescending(p => p.DateTime).FirstOrDefault();
                                if (package != null && package?.Status != null)
                                {
                                    if (package.Status.Type.Equals("D"))
                                        await _orderProcessingService.DeliverAsync(shipment, true);
                                }
                            }
                        }
                    }
                    else if (shippingMethod.Equals(ShipmentMethod.USPS.ToString()))
                    {
                        var trackInfo = await USPSShipmentDeliveredAsync(shipment);
                        if (trackInfo != null && trackInfo?.TrackSummary != null)
                        {
                            if (trackInfo.TrackSummary.Event.Contains("Delivered") && !trackInfo.TrackSummary.Event.Contains("In-Transit"))
                                await _orderProcessingService.DeliverAsync(shipment, true);
                        }
                    }
                    else if (shippingMethod.Equals(ShipmentMethod.FEDEX.ToString()))
                    {
                        var result = await FedExShipmentTrackingAsync(shipment);
                        if (result != null)
                        {
                            if (result.Errors?.Any() ?? false)
                                await _logger.ErrorAsync($"Shipment Tracking (FedEx): {string.Join(", ", result.Errors.Select(p => $"{p.Code} - {p.Message}"))} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                            else
                            {
                                var trackResponse = result.Response;
                                if (trackResponse != null)
                                {
                                    if (trackResponse.Errors?.Any() ?? false)
                                        await _logger.ErrorAsync($"Shipment Tracking (FedEx): {string.Join(", ", trackResponse.Errors.Select(p => $"{p.Code} - {p.Message}"))} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                                    else
                                    {
                                        if (trackResponse.Packages?.Any() ?? false)
                                        {
                                            var package = trackResponse.Packages.Where(p => p.TrackingNumber.Equals(shipment.TrackingNumber)).FirstOrDefault();
                                            if (package != null)
                                            {
                                                var packageResult = package.Results.Where(p => p.NumberInfo.TrackingNumber.Equals(shipment.TrackingNumber)).FirstOrDefault();
                                                if (packageResult.TrackingError != null)
                                                    await _logger.ErrorAsync($"Shipment Tracking (FedEx): {packageResult.TrackingError.Code} - {packageResult.TrackingError.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                                                else
                                                {
                                                    var status = packageResult.StatusDetail;
                                                    if (status != null && (status?.Code.Equals("DL") ?? false))
                                                        await _orderProcessingService.DeliverAsync(shipment, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else if (shippingMethod.Equals(ShipmentMethod.SPEEDEE.ToString()))
                    {
                        var result = await SPEEDEEShipmentDeliveredAsync(shipment);

                        if (result != null)
                        {
                            var trackResponse = result.header;
                            if (trackResponse != null)
                            {
                                if (trackResponse.status.Equals("Delivered"))
                                    await _orderProcessingService.DeliverAsync(shipment, true);
                            }
                        }
                    }
                    else if (shippingMethod.Equals(ShipmentMethod.DHL.ToString()))
                    {
                        var result = await DHLShipmentDeliveredAsync(shipment);

                        if (result != null)
                        {
                            var trackResponse = result?.shipments?.FirstOrDefault(s => s.TrackingNumber.Equals(shipment.TrackingNumber));
                            if (trackResponse != null)
                            {
                                if (trackResponse.status.Equals("DELIVERED"))
                                    await _orderProcessingService.DeliverAsync(shipment, true);
                            }
                        }
                    }
                    else
                    {
                        if (!shipment.TrackingNumber.StartsWith("1Z"))
                        {
                            var trackInfo = await USPSShipmentDeliveredAsync(shipment);

                            if (trackInfo != null && trackInfo?.TrackSummary != null)
                            {
                                if (trackInfo.TrackSummary.Event.Contains("Delivered") && !trackInfo.TrackSummary.Event.Contains("In-Transit"))
                                    await _orderProcessingService.DeliverAsync(shipment, true);
                            }
                        }
                        else
                        {
                            var (isSuccess, response) = await UPSShipmentDeliveredAsync(shipment);

                            if (isSuccess && response != null)
                            {
                                var trackResponse = response.Response;
                                if (trackResponse != null &&
                                    trackResponse?.Shipment != null && (trackResponse?.Shipment?.FirstOrDefault().Packages.Any() ?? false))
                                {
                                    var package = trackResponse?.Shipment?.FirstOrDefault().Packages?.FirstOrDefault()?.Activity?.OrderByDescending(p => p.DateTime).FirstOrDefault();
                                    if (package != null && package?.Status != null)
                                    {
                                        if (package.Status.Type.Equals("D"))
                                            await _orderProcessingService.DeliverAsync(shipment, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (shipments.Any())
            {
                _shipmentTrackingSettings.CurrentPageIndex += 1;
                await _settingService.SaveSettingAsync(_shipmentTrackingSettings);
            }
            else
            {
                _shipmentTrackingSettings.CurrentPageIndex = 0;
                await _settingService.SaveSettingAsync(_shipmentTrackingSettings);
            }
        }

        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipments
        /// </returns>
        public virtual async Task<IPagedList<Shipment>> GetAllNotDeliveredShipmentsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var shipments = await _shipmentRepository.GetAllPagedAsync(query =>
            {
                //restrict by generic attribute 
                query = from s in query
                        where _genericAttributeRepository.Table.Any(a =>
                            a.EntityId == s.Id && a.KeyGroup.Equals(nameof(Shipment)) && a.Key.Equals(ShippingMethod))
                        select s;

                //only shipped shipments
                query = query.Where(s => s.ShippedDateUtc.HasValue);

                //only not delivered shipments
                query = query.Where(s => !s.DeliveryDateUtc.HasValue);

                query = from s in query
                        join o in _orderRepository.Table on s.OrderId equals o.Id
                        where !o.Deleted
                        select s;

                //skip records was more then 6 month old and not able to track there shipping detail.
                var fromutc = DateTime.UtcNow.AddMonths(-4);
                query = query.Where(s => fromutc <= s.ShippedDateUtc);

                query = query.Distinct();

                query = query.OrderByDescending(s => s.ShippedDateUtc);

                return query;
            }, pageIndex, pageSize);

            return shipments;
        }

        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier, only shipments with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="shippingCountryId">Shipping country identifier; 0 to load all records</param>
        /// <param name="shippingStateId">Shipping state identifier; 0 to load all records</param>
        /// <param name="shippingCounty">Shipping county; null to load all records</param>
        /// <param name="shippingCity">Shipping city; null to load all records</param>
        /// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="loadNotShipped">A value indicating whether we should load only not shipped shipments</param>
        /// <param name="loadNotDelivered">A value indicating whether we should load only not delivered shipments</param>
        /// <param name="orderId">Order identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="shippingMethod">Shipping method; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipments
        /// </returns>
        public virtual async Task<IPagedList<Shipment>> GetAllShipmentsAsync(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCounty = null,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            bool loadNotReadyForPickup = false,
            bool loadNotDelivered = false,
            int orderId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            string shippingMethod = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var shipments = await _shipmentRepository.GetAllPagedAsync(query =>
            {
                if (orderId > 0)
                    query = query.Where(o => o.OrderId == orderId);

                if (!string.IsNullOrEmpty(trackingNumber))
                    query = query.Where(s => s.TrackingNumber.Contains(trackingNumber.Trim()));

                if (shippingCountryId > 0)
                    query = from s in query
                            join o in _orderRepository.Table on s.OrderId equals o.Id
                            where _addressRepository.Table.Any(a =>
                                a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                                a.CountryId == shippingCountryId)
                            select s;

                if (shippingStateId > 0)
                    query = from s in query
                            join o in _orderRepository.Table on s.OrderId equals o.Id
                            where _addressRepository.Table.Any(a =>
                                a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                                a.StateProvinceId == shippingStateId)
                            select s;

                if (!string.IsNullOrWhiteSpace(shippingCounty))
                    query = from s in query
                            join o in _orderRepository.Table on s.OrderId equals o.Id
                            where _addressRepository.Table.Any(a =>
                                a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                                a.County.Contains(shippingCounty))
                            select s;

                if (!string.IsNullOrWhiteSpace(shippingCity))
                    query = from s in query
                            join o in _orderRepository.Table on s.OrderId equals o.Id
                            where _addressRepository.Table.Any(a =>
                                a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                                a.City.Contains(shippingCity))
                            select s;

                if (!string.IsNullOrWhiteSpace(shippingMethod))
                    query = from s in query
                            where _genericAttributeRepository.Table.Any(a =>
                                a.EntityId == s.Id && a.KeyGroup.Equals(nameof(Shipment)) && a.Key.Equals(ShippingMethod) && a.Value.Equals(shippingMethod))
                            select s;

                if (loadNotShipped)
                    query = query.Where(s => !s.ShippedDateUtc.HasValue);

                if (loadNotDelivered)
                    query = query.Where(s => !s.DeliveryDateUtc.HasValue);

                if (createdFromUtc.HasValue)
                    query = query.Where(s => createdFromUtc.Value <= s.CreatedOnUtc);

                if (createdToUtc.HasValue)
                    query = query.Where(s => createdToUtc.Value >= s.CreatedOnUtc);

                query = from s in query
                        join o in _orderRepository.Table on s.OrderId equals o.Id
                        where !o.Deleted
                        select s;

                query = query.Distinct();

                if (vendorId > 0)
                {
                    var queryVendorOrderItems = from orderItem in _orderItemRepository.Table
                                                join p in _productRepository.Table on orderItem.ProductId equals p.Id
                                                where p.VendorId == vendorId
                                                select orderItem.Id;

                    query = from s in query
                            join si in _siRepository.Table on s.Id equals si.ShipmentId
                            where queryVendorOrderItems.Contains(si.OrderItemId)
                            select s;

                    query = query.Distinct();
                }

                if (warehouseId > 0)
                {
                    query = from s in query
                            join si in _siRepository.Table on s.Id equals si.ShipmentId
                            where si.WarehouseId == warehouseId
                            select s;

                    query = query.Distinct();
                }

                query = query.OrderByDescending(s => s.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return shipments;
        }

        /// <summary>
        /// Gets an URL for a page to show tracking info (third party tracking page).
        /// </summary>
        /// <param name="trackingNumber">The tracking number to track.</param>
        /// <param name="shippingMethod">The shipping method.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the uRL of a tracking page.
        /// </returns>
        public virtual Task<string> GetTrackingUrlAsync(string trackingNumber, string shippingMethod)
        {
            if (!string.IsNullOrEmpty(shippingMethod) && !string.IsNullOrWhiteSpace(trackingNumber))
            {
                trackingNumber = trackingNumber.Trim();
                //Find tracking url token to replace with new url
                if (shippingMethod.Equals(ShipmentMethod.UPS.ToString()))
                {
                    return Task.FromResult($"https://www.ups.com/track?&tracknum={trackingNumber}");
                }
                else if (shippingMethod.Equals(ShipmentMethod.USPS.ToString()))
                {
                    return Task.FromResult($"https://tools.usps.com/go/TrackConfirmAction?tLabels={trackingNumber}");
                }
                else if (shippingMethod.Equals(ShipmentMethod.FEDEX.ToString()))
                {
                    return Task.FromResult($"https://www.fedex.com/apps/fedextrack/?action=track&tracknumbers={trackingNumber}");
                }
                else if (shippingMethod.Equals(ShipmentMethod.SPEEDEE.ToString()))
                {
                    return Task.FromResult($"https://speedeedelivery.com/track-a-shipment/?barcodes={trackingNumber}");
                }
                else if (shippingMethod.Equals(ShipmentMethod.DHL.ToString()))
                {
                    return Task.FromResult($"https://www.dhl.com/global-en/home/tracking/tracking-express.html?submit=1&tracking-id={trackingNumber}");
                }
            }

            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// Gets all events for a tracking number.
        /// </summary>
        /// <param name="shipment">The shipment</param>
        /// <param name="shippingMethod">The shipping method</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of Shipment Events.
        /// </returns>
        public virtual async Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(Shipment shipment, string shippingMethod)
        {
            if (string.IsNullOrEmpty(shipment.TrackingNumber))
                return new List<ShipmentStatusEvent>();

            if (!string.IsNullOrEmpty(shippingMethod) && !string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                if (shippingMethod.Equals(ShipmentMethod.UPS.ToString()))
                {
                    var (isSuccess, response) = await UPSShipmentDeliveredAsync(shipment);

                    if (isSuccess && response != null)
                    {
                        var trackResponse = response.Response;
                        if (trackResponse != null &&
                            trackResponse?.Shipment != null && (trackResponse?.Shipment?.FirstOrDefault().Packages.Any() ?? false))
                        {
                            var activities = trackResponse?.Shipment?.FirstOrDefault().Packages?.FirstOrDefault()?.Activity.Where(activity => activity != null).ToList();
                            return await activities.SelectAwait(async activity => await PrepareShipmentStatusEventAsync(activity)).ToListAsync();
                        }
                    }
                }
                else if (shippingMethod.Equals(ShipmentMethod.USPS.ToString()))
                {
                    var trackInfo = await USPSShipmentDeliveredAsync(shipment);
                    if (trackInfo?.TrackDetails?.Any() ?? false)
                        return trackInfo.TrackDetails
                            .Select(x => new ShipmentStatusEvent
                            {
                                Date = x.Date,
                                EventName = x.Event,
                                Location = x.City,
                                CountryCode = x.Country
                            })
                            .ToList();
                }
                else if (shippingMethod.Equals(ShipmentMethod.FEDEX.ToString()))
                {
                    var result = await FedExShipmentTrackingAsync(shipment, true);
                    if (result != null)
                    {
                        if (result.Errors?.Any() ?? false)
                            await _logger.ErrorAsync($"Shipment Tracking (FedEx): {string.Join(", ", result.Errors.Select(p => $"{p.Code} - {p.Message}"))} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                        else
                        {
                            var trackResponse = result.Response;
                            if (trackResponse != null)
                            {
                                if (trackResponse.Errors?.Any() ?? false)
                                    await _logger.ErrorAsync($"Shipment Tracking (FedEx): {string.Join(", ", trackResponse.Errors.Select(p => $"{p.Code} - {p.Message}"))} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                                else
                                {
                                    if (trackResponse.Packages?.Any() ?? false)
                                    {
                                        var package = trackResponse.Packages.Where(p => p.TrackingNumber.Equals(shipment.TrackingNumber)).FirstOrDefault();
                                        if (package != null)
                                        {
                                            var packageResult = package.Results.Where(p => p.NumberInfo.TrackingNumber.Equals(shipment.TrackingNumber)).FirstOrDefault();
                                            if (packageResult.TrackingError != null)
                                                await _logger.ErrorAsync($"Shipment Tracking (FedEx): {packageResult.TrackingError.Code} - {packageResult.TrackingError.Message} #{shipment.Id} - Tracking Number : {shipment.TrackingNumber}");
                                            else
                                            {
                                                return packageResult.Events?.Where(trackEvent => trackEvent != null)
                                                .Select(trackEvent => new ShipmentStatusEvent
                                                {
                                                    EventName = $"{trackEvent.Description} ({trackEvent.Type})",
                                                    Location = string.Join(", ", new string[]
                                                    {
                                                        trackEvent.ScanLocation?.City ?? string.Empty,
                                                        $"{trackEvent.ScanLocation?.StateOrProvince} {trackEvent.ScanLocation?.PostalCode}" ?? string.Empty,
                                                    }.Where(p => !string.IsNullOrWhiteSpace(p))),
                                                    CountryCode = trackEvent.ScanLocation?.CountryCode,
                                                    Date = trackEvent?.DateTime
                                                }).ToList();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (shippingMethod.Equals(ShipmentMethod.SPEEDEE.ToString()))
                {
                    var result = await SPEEDEEShipmentDeliveredAsync(shipment);
                    if (result != null)
                    {
                        var trackResponse = result.detail;
                        if (trackResponse != null)
                        {
                            if (trackResponse?.Any() ?? false)
                                return trackResponse
                                    .Select(x => new ShipmentStatusEvent
                                    {
                                        Date = x.DateTime,
                                        EventName = x.activity,
                                        Location = x.scanloc?.Split(',').FirstOrDefault(),
                                        CountryCode = x.scanloc?.Split(',').LastOrDefault()
                                    })
                                    .ToList();
                        }
                    }
                }
                else if (shippingMethod.Equals(ShipmentMethod.DHL.ToString()))
                {
                    var result = await DHLShipmentDeliveredAsync(shipment);
                    if (result != null)
                    {
                        var trackResponse = result?.shipments?.FirstOrDefault(s => s.TrackingNumber.Equals(shipment.TrackingNumber));
                        if (trackResponse != null)
                        {
                            if (trackResponse?.status.Any() ?? false)
                                return trackResponse?.status
                                    .Select(x => new ShipmentStatusEvent
                                    {
                                        Date = x.DateTime,
                                        EventName = $"{x.status} ({x.statusCode})",
                                        Location = $"{x.location?.address?.addressLocality}",
                                        CountryCode = x.location?.address?.countryCode
                                    })
                                    .ToList();
                        }
                    }
                }
                else
                {
                    if (!shipment.TrackingNumber.StartsWith("1Z"))
                    {
                        var trackInfo = await USPSShipmentDeliveredAsync(shipment);
                        if (trackInfo?.TrackDetails?.Any() ?? false)
                            return trackInfo.TrackDetails
                                .Select(x => new ShipmentStatusEvent
                                {
                                    Date = x.Date,
                                    EventName = x.Event,
                                    Location = x.City,
                                    CountryCode = x.Country
                                })
                                .ToList();
                    }
                    else
                    {
                        var (isSucess, response) = await UPSShipmentDeliveredAsync(shipment);
                        if (isSucess && response != null)
                        {
                            var trackResponse = response.Response;
                            if (trackResponse != null &&
                                trackResponse?.Shipment != null && (trackResponse?.Shipment?.FirstOrDefault().Packages.Any() ?? false))
                            {
                                var activities = trackResponse?.Shipment?.FirstOrDefault().Packages?.FirstOrDefault()?.Activity.Where(activity => activity != null).ToList();
                                return await activities.SelectAwait(async activity => await PrepareShipmentStatusEventAsync(activity)).ToListAsync();
                            }
                        }
                    }
                }
            }

            return new List<ShipmentStatusEvent>();
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="purchaseOrderNumber">Search in purchase order number. Leave empty to load all records.</param>
        /// <param name="billingZipPostalCode">Billing zip / postal code. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the orders
        /// </returns>
        public virtual async Task<IPagedList<Order>> SearchOrdersAsync(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, string purchaseOrderNumber = null, string billingZipPostalCode = "", string company = null,
            int pageIndex = 0,int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _orderRepository.Table;

            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);

            if (vendorId > 0)
            {
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.VendorId == vendorId
                        select o;

                query = query.Distinct();
            }

            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);

            if (productId > 0)
            {
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        where oi.ProductId == productId
                        select o;

                query = query.Distinct();
            }

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        join pwi in _productWarehouseInventoryRepository.Table on p.Id equals pwi.ProductId into ps
                        from pwi in ps.DefaultIfEmpty()
                        where
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (p.ManageInventoryMethodId == manageStockInventoryMethodId && p.UseMultipleWarehouses && pwi.WarehouseId == warehouseId) ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((p.ManageInventoryMethodId != manageStockInventoryMethodId || !p.UseMultipleWarehouses) && p.WarehouseId == warehouseId)
                        select o;

                query = query.Distinct();
            }

            if (!string.IsNullOrWhiteSpace(company))
            {
                query = from o in query
                        join c in _customerRepository.Table on o.CustomerId equals c.Id
                        where c.Company != null && c.Company.Contains(company)
                        select o;
            }
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => _orderNoteRepository.Table.Any(oNote => oNote.OrderId == o.Id && oNote.Note.Contains(orderNotes)));

            if (!string.IsNullOrEmpty(purchaseOrderNumber))
            {
                var key = $"<key>{await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber")}</key>";
                query = query.Where(o => o.CustomValuesXml.Contains(key) && o.CustomValuesXml.Contains(purchaseOrderNumber));
            }

            query = from o in query
                    join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                    where
                        (billingCountryId <= 0 || (oba.CountryId == billingCountryId)) &&
                        (string.IsNullOrEmpty(billingPhone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(billingPhone))) &&
                        (string.IsNullOrEmpty(billingEmail) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                        (string.IsNullOrEmpty(billingLastName) || (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName))) &&
                        (string.IsNullOrEmpty(billingZipPostalCode) || (!string.IsNullOrEmpty(oba.ZipPostalCode) && oba.ZipPostalCode.Contains(billingZipPostalCode)))
                    select o;

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="osIds">Order status identifiers</param>
        /// <param name="psIds">Payment status identifiers</param>
        /// <param name="ssIds">Shipping status identifiers</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>        
        /// <param name="purchaseOrderNumber">Search in purchase order number. Leave empty to load all records.</param>
        /// <param name="billingZipPostalCode">Billing zip / postal code. Leave empty to load all records.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<OrderAverageReportLine> GetOrderAverageReportLineAsync(int storeId = 0,
            int vendorId = 0, int productId = 0, int warehouseId = 0, int billingCountryId = 0,
            int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null,
            string purchaseOrderNumber = null, string billingZipPostalCode = "")
        {
            var query = _orderRepository.Table;

            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (orderId > 0)
                query = query.Where(o => o.Id == orderId);

            if (vendorId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.VendorId == vendorId
                        select o;

            if (productId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        where oi.ProductId == productId
                        select o;

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        join pwi in _productWarehouseInventoryRepository.Table on p.Id equals pwi.ProductId
                        where
                            //"Use multiple warehouses" enabled
                            //we search in each warehouse
                            (p.ManageInventoryMethodId == manageStockInventoryMethodId && p.UseMultipleWarehouses && pwi.WarehouseId == warehouseId) ||
                            //"Use multiple warehouses" disabled
                            //we use standard "warehouse" property
                            ((p.ManageInventoryMethodId != manageStockInventoryMethodId || !p.UseMultipleWarehouses) && p.WarehouseId == warehouseId)
                        select o;
            }

            query = from o in query
                    join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                    where
                        (billingCountryId <= 0 || (oba.CountryId == billingCountryId)) &&
                        (string.IsNullOrEmpty(billingPhone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(billingPhone))) &&
                        (string.IsNullOrEmpty(billingEmail) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                        (string.IsNullOrEmpty(billingLastName) || (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName))) &&
                        (string.IsNullOrEmpty(billingZipPostalCode) || (!string.IsNullOrEmpty(oba.ZipPostalCode) && oba.ZipPostalCode.Contains(billingZipPostalCode)))
                    select o;

            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);

            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

            if (!string.IsNullOrEmpty(orderNotes))
                query = from o in query
                        join n in _orderNoteRepository.Table on o.Id equals n.OrderId
                        where n.Note.Contains(orderNotes)
                        select o;

            if (!string.IsNullOrEmpty(purchaseOrderNumber))
            {
                var key = $"<key>{await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber")}</key>";
                query = query.Where(o => o.CustomValuesXml.Contains(key) && o.CustomValuesXml.Contains(purchaseOrderNumber));
            }

            var item = await (from oq in query
                              group oq by 1
                into result
                              select new
                              {
                                  OrderCount = result.Count(),
                                  OrderShippingExclTaxSum = result.Sum(o => o.OrderShippingExclTax),
                                  OrderPaymentFeeExclTaxSum = result.Sum(o => o.PaymentMethodAdditionalFeeExclTax),
                                  OrderTaxSum = result.Sum(o => o.OrderTax),
                                  OrderTotalSum = result.Sum(o => o.OrderTotal),
                                  OrederRefundedAmountSum = result.Sum(o => o.RefundedAmount),
                              }).Select(r => new OrderAverageReportLine
                              {
                                  CountOrders = r.OrderCount,
                                  SumShippingExclTax = r.OrderShippingExclTaxSum,
                                  OrderPaymentFeeExclTaxSum = r.OrderPaymentFeeExclTaxSum,
                                  SumTax = r.OrderTaxSum,
                                  SumOrders = r.OrderTotalSum,
                                  SumRefundedAmount = r.OrederRefundedAmountSum
                              })
                .FirstOrDefaultAsync();

            item ??= new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                OrderPaymentFeeExclTaxSum = decimal.Zero,
                SumTax = decimal.Zero,
                SumOrders = decimal.Zero
            };
            return item;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="purchaseOrderNumber">Search in purchase order number. Leave empty to load all records.</param>
        /// <param name="billingZipPostalCode">Billing zip / postal code. Leave empty to load all records.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<decimal> ProfitReportAsync(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null,
            string purchaseOrderNumber = null, string billingZipPostalCode = "")
        {
            var dontSearchPhone = string.IsNullOrEmpty(billingPhone);
            var dontSearchEmail = string.IsNullOrEmpty(billingEmail);
            var dontSearchLastName = string.IsNullOrEmpty(billingLastName);
            var dontSearchOrderNotes = string.IsNullOrEmpty(orderNotes);
            var dontSearchPaymentMethods = string.IsNullOrEmpty(paymentMethodSystemName);
            var dontPurchaseOrderNumber = string.IsNullOrEmpty(purchaseOrderNumber);
            var purchaseOrderKey = $"<key>{await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber")}</key>";
            var dontBillingZipPostalCode = string.IsNullOrEmpty(billingZipPostalCode);

            var orders = _orderRepository.Table;
            if (osIds != null && osIds.Any())
                orders = orders.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                orders = orders.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                orders = orders.Where(o => ssIds.Contains(o.ShippingStatusId));

            var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

            var query = from orderItem in _orderItemRepository.Table
                        join o in orders on orderItem.OrderId equals o.Id
                        join p in _productRepository.Table on orderItem.ProductId equals p.Id
                        join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                              (orderId == 0 || orderId == o.Id) &&
                              (billingCountryId == 0 || (oba.CountryId == billingCountryId)) &&
                              (dontSearchPaymentMethods || paymentMethodSystemName == o.PaymentMethodSystemName) &&
                              (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              !o.Deleted &&
                              (vendorId == 0 || p.VendorId == vendorId) &&
                              (productId == 0 || orderItem.ProductId == productId) &&
                              (warehouseId == 0 ||
                                  //"Use multiple warehouses" enabled
                                  //we search in each warehouse
                                  p.ManageInventoryMethodId == manageStockInventoryMethodId &&
                                  p.UseMultipleWarehouses &&
                                  _productWarehouseInventoryRepository.Table.Any(pwi =>
                                      pwi.ProductId == orderItem.ProductId && pwi.WarehouseId == warehouseId)
                                  ||
                                  //"Use multiple warehouses" disabled
                                  //we use standard "warehouse" property
                                  (p.ManageInventoryMethodId != manageStockInventoryMethodId ||
                                   !p.UseMultipleWarehouses) &&
                                  p.WarehouseId == warehouseId) &&
                              //we do not ignore deleted products when calculating order reports                            
                              (dontSearchPhone || (!string.IsNullOrEmpty(oba.PhoneNumber) &&
                                                   oba.PhoneNumber.Contains(billingPhone))) &&
                              (dontSearchEmail || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                              (dontSearchLastName ||
                               (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName))) &&
                               (dontBillingZipPostalCode ||
                               (!string.IsNullOrEmpty(oba.ZipPostalCode) && oba.ZipPostalCode.Contains(billingZipPostalCode))) &&
                              (dontSearchOrderNotes || _orderNoteRepository.Table.Any(oNote =>
                                   oNote.OrderId == o.Id && oNote.Note.Contains(orderNotes))) &&
                                (dontPurchaseOrderNumber || (o.CustomValuesXml.Contains(purchaseOrderKey) && o.CustomValuesXml.Contains(purchaseOrderNumber)))
                        select orderItem;

            var productCost = Convert.ToDecimal(await query.SumAsync(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var reportSummary = await GetOrderAverageReportLineAsync(
                storeId,
                vendorId,
                productId,
                warehouseId,
                billingCountryId,
                orderId,
                paymentMethodSystemName,
                osIds,
                psIds,
                ssIds,
                startTimeUtc,
                endTimeUtc,
                billingPhone,
                billingEmail,
                billingLastName,
                orderNotes,
                purchaseOrderNumber,
                billingZipPostalCode);

            var profit = reportSummary.SumOrders
                         - reportSummary.SumShippingExclTax
                         - reportSummary.OrderPaymentFeeExclTaxSum
                         - reportSummary.SumTax
                         - reportSummary.SumRefundedAmount
                         - productCost;
            return profit;
        }

        #endregion
    }
}
