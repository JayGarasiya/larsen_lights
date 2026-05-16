using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a fedex tracking request model
    /// </summary>
    public class FedExTrackingRequest
    {
        [JsonProperty(PropertyName = "includeDetailedScans")]
        public bool DetailedScans { get; set; }

        [JsonProperty(PropertyName = "trackingInfo")]
        public List<TrackingInfo> Request { get; set; }

        #region Nested classes

        public class TrackingInfo
        {
            [JsonProperty(PropertyName = "trackingNumberInfo")]
            public NumberInfo TrackNumberDetail { get; set; }

            #region Nested classes

            public class NumberInfo
            {
                [JsonProperty(PropertyName = "trackingNumber")]
                public string TrackingNumber { get; set; }
            }

            #endregion
        }

        #endregion
    }
}
