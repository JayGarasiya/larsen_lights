using Newtonsoft.Json;
using System.Globalization;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a DHL respone model
    /// </summary>
    public class DHLRespone
    {
        public List<Shipment> shipments { get; set; }

        #region Nested classes

        public class Shipment
        {
            [JsonProperty(PropertyName = "id")]
            public string TrackingNumber { get; set; }

            public List<Event> status { get; set; }

            #region Nested classes

            public class Event
            {
                [JsonProperty(PropertyName = "timestamp")]
                public string EventDate { get; set; }

                public Location location { get; set; }

                public string statusCode { get; set; }

                public string status { get; set; }

                [JsonIgnore]
                public DateTime DateTime => DateTime.ParseExact(EventDate, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

                #region Nested classes

                public class Location
                {
                    public LocationAddress address { get; set; }

                    #region Nested classes

                    public class LocationAddress
                    {
                        public string countryCode { get; set; }

                        public string addressLocality { get; set; }
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
