//using Newtonsoft.Json;

//namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
//{
//    /// <summary>
//    /// Represents an UPS track request
//    /// </summary>
//    public class UPSTrackRequest
//    {
//        [JsonProperty(PropertyName = "UPSSecurity")]
//        public UPSSecurity Security { get; set; }

//        [JsonProperty(PropertyName = "TrackRequest")]
//        public TrackRequest Request { get; set; }

//        #region Nested Classes

//        public class UPSSecurity
//        {
//            [JsonProperty(PropertyName = "UsernameToken")]
//            public UsernameToken Token { get; set; }

//            [JsonProperty(PropertyName = "ServiceAccessToken")]
//            public ServiceAccessToken AccessToken { get; set; }

//            #region Nested Classes

//            public class UsernameToken
//            {
//                [JsonProperty(PropertyName = "Username")]
//                public string Username { get; set; }

//                [JsonProperty(PropertyName = "Password")]
//                public string Password { get; set; }
//            }

//            public class ServiceAccessToken
//            {
//                [JsonProperty(PropertyName = "AccessLicenseNumber")]
//                public string AccessLicenseNumber { get; set; }
//            }

//            #endregion
//        }

//        public class TrackRequest
//        {
//            [JsonProperty(PropertyName = "Request")]
//            public RequestOptions Request { get; set; }

//            [JsonProperty(PropertyName = "InquiryNumber")]
//            public string InquiryNumber { get; set; }

//            #region Nested Classes

//            public class RequestOptions
//            {
//                [JsonProperty(PropertyName = "RequestOption")]
//                public string RequestOption { get; set; }

//                [JsonProperty(PropertyName = "TransactionReference")]
//                public TransactionReference TransactionRef { get; set; }

//                #region Nested Classes

//                public class TransactionReference
//                {
//                    [JsonProperty(PropertyName = "CustomerContext")]
//                    public string CustomerContext { get; set; }
//                }

//                #endregion
//            }

//            #endregion
//        }

//        #endregion
//    }
//}
