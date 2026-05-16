using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a fedex auth response model
    /// </summary>
    public class FedExAuthResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<FedExAuthErrors> Errors { get; set; }

        #region Nested classes 

        public class FedExAuthErrors
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
        }

        #endregion
    }
}
