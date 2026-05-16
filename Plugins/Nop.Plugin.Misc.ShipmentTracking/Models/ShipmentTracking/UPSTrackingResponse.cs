using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents an UPS tracking response
    /// </summary>
    public class UPSTrackingResponse
    {
        [JsonProperty(PropertyName = "TrackResponse")]
        public TrackResponse Response { get; set; }

        #region Nested classes

        public class TrackResponse
        {
            [JsonProperty("shipment")]
            public List<Shipments> Shipment { get; set; }

            #region Nested classes

            public class Shipments
            {
                public Shipments()
                {
                    Packages = new List<PackageInfo>();
                }

                public object Package { get; set; }

                public List<PackageInfo> Packages { get; set; }

                #region Nested classes

                public class PackageInfo
                {
                    [JsonConverter(typeof(UPSJsonConverter<ActivityLogs>))]
                    //[JsonProperty(PropertyName = "activity")]
                    public List<ActivityLogs> Activity { get; set; }

                    #region Nested classes

                    public class ActivityLogs
                    {
                        [JsonProperty(PropertyName = "location")]
                        public LocationInfo ActivityLocation { get; set; }

                        public StatusInfo Status { get; set; }

                        public string Date { get; set; }

                        public string Time { get; set; }

                        [JsonIgnore]
                        public DateTime DateTime => new DateTime(Convert.ToInt32(this.Date.Substring(0, 4)), Convert.ToInt32(this.Date.Substring(4, 2)), Convert.ToInt32(this.Date.Substring(6, 2)), Convert.ToInt32(this.Time.Substring(0, 2)), Convert.ToInt32(this.Time.Substring(2, 2)), Convert.ToInt32(this.Time.Substring(4, 2)));

                        #region Nested classes

                        public class LocationInfo
                        {
                            public Location Address { get; set; }

                            #region Nested classes

                            public class Location
                            {
                                public string AddressLine { get; set; }

                                public string City { get; set; }

                                [JsonProperty(PropertyName = "stateProvince")]
                                public string StateProvinceCode { get; set; }

                                public string PostalCode { get; set; }

                                public string CountryCode { get; set; }
                            }

                            #endregion
                        }

                        public class StatusInfo
                        {
                            public string Type { get; set; }

                            public string Code { get; set; }
                        }

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
