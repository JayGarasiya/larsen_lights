using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ShipmentTracking.Models
{
    /// <summary>
    /// Represents an configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.EnableUPSMethod")]
        public bool EnableUPSMethod { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.UPSTrackingApiUrl")]
        public string UPSTrackingApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.UPSClientId")]
        public string UPSClientId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.UPSClientSecret")]
        public string UPSClientSecret { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.UPSAuthApiUrl")]
        public string UPSAuthApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.EnableUSPSMethod")]
        public bool EnableUSPSMethod { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.USPSTrackingApiUrl")]
        public string USPSTrackingApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.USPSUsername")]
        public string USPSUsername { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.USPSPassword")]
        public string USPSPassword { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.EnableFedExMethod")]
        public bool EnableFedExMethod { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.FedExTrackingApiUrl")]
        public string FedExTrackingApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.FedExClientId")]
        public string FedExClientId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.FedExClientSecret")]
        public string FedExClientSecret { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.EnableSpeedeeMethod")]
        public bool EnableSpeedeeMethod { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.SpeedeeTrackingApiUrl")]
        public string SpeedeeTrackingApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.SpeedeeAccount")]
        public string SpeedeeAccount { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.SpeedeePassword")]
        public string SpeedeePassword { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.EnableDHLMethod")]
        public bool EnableDHLMethod { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.DHLTrackingApiUrl")]
        public string DHLTrackingApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.DHLConsumerKey")]
        public string DHLConsumerKey { get; set; }

        #endregion
    }
}