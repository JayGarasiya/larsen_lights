using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a fedex tracking response model
    /// </summary>
    public class FedExTrackingResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public List<FedExTrackErrors> Errors { get; set; }

        [JsonProperty(PropertyName = "output")]
        public TrackPackagesResponse Response { get; set; }

        #region Nested classes 

        public class TrackPackagesResponse
        {
            [JsonProperty(PropertyName = "alerts")]
            public List<Error> Errors { get; set; }

            [JsonProperty(PropertyName = "completeTrackResults")]
            public List<Package> Packages { get; set; }

            #region Nested classes 

            public class Error
            {
                [JsonProperty(PropertyName = "code")]
                public string Code { get; set; }

                [JsonProperty(PropertyName = "message")]
                public string Message { get; set; }
            }

            public class Package
            {
                [JsonProperty(PropertyName = "trackingNumber")]
                public string TrackingNumber { get; set; }

                [JsonProperty(PropertyName = "trackResults")]
                public List<TrackResults> Results { get; set; }

                #region Nested classes 

                public class TrackResults
                {
                    [JsonProperty(PropertyName = "trackingNumberInfo")]
                    public TrackingNumberInfo NumberInfo { get; set; }

                    [JsonProperty(PropertyName = "latestStatusDetail")]
                    public LatestEvent StatusDetail { get; set; }

                    [JsonProperty(PropertyName = "scanEvents")]
                    public List<Event> Events { get; set; }

                    [JsonProperty(PropertyName = "error")]
                    public Error TrackingError { get; set; }
                }

                public class TrackingNumberInfo
                {
                    [JsonProperty(PropertyName = "trackingNumber")]
                    public string TrackingNumber { get; set; }
                }

                public class LatestEvent
                {
                    [JsonProperty(PropertyName = "code")]
                    public string Code { get; set; }
                }

                public class Event
                {
                    [JsonProperty(PropertyName = "date")]
                    public string Date { get; set; }

                    [JsonIgnore]
                    public DateTime? DateTime => (!string.IsNullOrEmpty(this.Date) ? new DateTime(Convert.ToInt32(this.Date.Substring(0, 4)), Convert.ToInt32(this.Date.Substring(5, 2)), Convert.ToInt32(this.Date.Substring(8, 2)), Convert.ToInt32(this.Date.Substring(11, 2)), Convert.ToInt32(this.Date.Substring(14, 2)), Convert.ToInt32(this.Date.Substring(17, 2))) : null);

                    [JsonProperty(PropertyName = "eventType")]
                    public string Type { get; set; }

                    [JsonProperty(PropertyName = "eventDescription")]
                    public string Description { get; set; }

                    [JsonProperty(PropertyName = "scanLocation")]
                    public Location ScanLocation { get; set; }

                }

                public class Location
                {
                    [JsonProperty(PropertyName = "city")]
                    public string City { get; set; }

                    [JsonProperty(PropertyName = "stateOrProvinceCode")]
                    public string StateOrProvince { get; set; }

                    [JsonProperty(PropertyName = "postalCode")]
                    public string PostalCode { get; set; }

                    [JsonProperty(PropertyName = "countryCode")]
                    public string CountryCode { get; set; }
                }

                #endregion
            }

            #endregion
        }

        public class FedExTrackErrors
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
        }

        #endregion
    }
}
