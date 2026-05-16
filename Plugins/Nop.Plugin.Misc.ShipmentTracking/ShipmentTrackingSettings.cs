using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ShipmentTracking
{
    /// <summary>
    /// Represents shipment tracking plugin settings
    /// </summary>
    public class ShipmentTrackingSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether UPS tracking method is enabled
        /// </summary>
        public bool EnableUPSMethod { get; set; }

        /// <summary>
        /// Gets or sets UPS tracking API URL
        /// </summary>
        public string UPSTrackingApiUrl { get; set; }

        /// <summary>
        /// Gets or sets UPS authentication API URL
        /// </summary>
        public string UPSAuthApiUrl { get; set; }

        /// <summary>
        /// Gets or sets UPS authentication token
        /// </summary>
        public string UPSAuthToken { get; set; }

        /// <summary>
        /// Gets or sets UPS authentication token generated date and time
        /// </summary>
        public DateTime UPSAuthTokenGenerated { get; set; }

        /// <summary>
        /// Gets or sets UPS client identifier
        /// </summary>
        public string UPSClientId { get; set; }

        /// <summary>
        /// Gets or sets UPS client secret
        /// </summary>
        public string UPSClientSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether USPS tracking method is enabled
        /// </summary>
        public bool EnableUSPSMethod { get; set; }

        /// <summary>
        /// Gets or sets USPS tracking API URL
        /// </summary>
        public string USPSTrackingApiUrl { get; set; }

        /// <summary>
        /// Gets or sets USPS username
        /// </summary>
        public string USPSUsername { get; set; }

        /// <summary>
        /// Gets or sets USPS password
        /// </summary>
        public string USPSPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether FedEx tracking method is enabled
        /// </summary>
        public bool EnableFedExMethod { get; set; }

        /// <summary>
        /// Gets or sets FedEx tracking API URL
        /// </summary>
        public string FedExTrackingApiUrl { get; set; }

        /// <summary>
        /// Gets or sets FedEx client identifier
        /// </summary>
        public string FedExClientId { get; set; }

        /// <summary>
        /// Gets or sets FedEx client secret
        /// </summary>
        public string FedExClientSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Speedee tracking method is enabled
        /// </summary>
        public bool EnableSpeedeeMethod { get; set; }

        /// <summary>
        /// Gets or sets Speedee tracking API URL
        /// </summary>
        public string SpeedeeTrackingApiUrl { get; set; }

        /// <summary>
        /// Gets or sets Speedee account username
        /// </summary>
        public string SpeedeeAccount { get; set; }

        /// <summary>
        /// Gets or sets Speedee account password
        /// </summary>
        public string SpeedeePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DHL tracking method is enabled
        /// </summary>
        public bool EnableDHLMethod { get; set; }

        /// <summary>
        /// Gets or sets DHL tracking API URL
        /// </summary>
        public string DHLTrackingApiUrl { get; set; }

        /// <summary>
        /// Gets or sets DHL consumer key
        /// </summary>
        public string DHLConsumerKey { get; set; }

        /// <summary>
        /// Gets or sets the current page index for tracking result pagination
        /// </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>
        /// Validates UPS credentials and configuration
        /// </summary>
        public bool ValidateUPSCredentials()
        {
            if (!this.EnableUPSMethod)
                return false;

            if (string.IsNullOrEmpty(this.UPSTrackingApiUrl)
                || string.IsNullOrEmpty(this.UPSAuthApiUrl) || string.IsNullOrEmpty(this.UPSClientId) || string.IsNullOrEmpty(this.UPSClientSecret))
                return false;

            return true;
        }

        /// <summary>
        /// Validates USPS credentials and configuration
        /// </summary>
        public bool ValidateUSPSCredentials()
        {
            if (!this.EnableUSPSMethod)
                return false;

            if (string.IsNullOrEmpty(this.USPSUsername) || string.IsNullOrEmpty(this.USPSPassword) || string.IsNullOrEmpty(this.USPSTrackingApiUrl))
                return false;

            return true;
        }

        /// <summary>
        /// Validates FedEx credentials and configuration
        /// </summary>
        public bool ValidateFedExCredentials()
        {
            if (!this.EnableFedExMethod)
                return false;

            if (string.IsNullOrEmpty(this.FedExTrackingApiUrl) || string.IsNullOrEmpty(this.FedExClientId) || string.IsNullOrEmpty(this.FedExClientSecret))
                return false;

            return true;
        }

        /// <summary>
        /// Validates Speedee credentials and configuration
        /// </summary>
        public bool ValidateSpeedeeCredentials()
        {
            if (!this.EnableSpeedeeMethod)
                return false;

            if (string.IsNullOrEmpty(this.SpeedeeAccount) || string.IsNullOrEmpty(this.SpeedeePassword) || string.IsNullOrEmpty(this.SpeedeeTrackingApiUrl))
                return false;

            return true;
        }

        /// <summary>
        /// Validates DHL credentials and configuration
        /// </summary>
        public bool ValidateDHLCredentials()
        {
            if (!this.EnableDHLMethod)
                return false;

            if (string.IsNullOrEmpty(this.DHLConsumerKey) || string.IsNullOrEmpty(this.DHLTrackingApiUrl))
                return false;

            return true;
        }
    }
}
