using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a speedee response model
    /// </summary>
    public class SpeedeeRespone
    {
        public Header header { get; set; }

        public List<Detail> detail { get; set; }

        #region Nested classes

        public class Header
        {
            public string status { get; set; }
        }

        public class Detail
        {
            [JsonProperty(PropertyName = "scandate")]
            public string Date { get; set; }

            [JsonProperty(PropertyName = "scantime")]
            public string Time { get; set; }

            public string scanloc { get; set; }

            public string activity { get; set; }

            [JsonIgnore]
            public DateTime DateTime => new DateTime(Convert.ToInt32(this.Date.Substring(6, 4)), Convert.ToInt32(this.Date.Substring(0, 2)), Convert.ToInt32(this.Date.Substring(3, 2)), Convert.ToInt32(this.Time.Substring(0, 2)), Convert.ToInt32(this.Time.Substring(3, 2)), Convert.ToInt32(this.Time.Substring(6, 2)));

        }

        #endregion
    }
}
