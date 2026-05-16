using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Evance.Models
{
    /// <summary>
    /// Represents a payment api response model
    /// </summary>
    public class PaymentApiResponseModel
    {
        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("responsetext")]
        public string ResponseMessage { get; set; }

        [JsonProperty("authcode")]
        public string AuthCode { get; set; }

        [JsonProperty("transactionid")]
        public string TransactionId { get; set; }

        [JsonProperty("avsresponse")]
        public string AvsResponse { get; set; }

        [JsonProperty("cvvresponse")]
        public string CvvResponse { get; set; }

        [JsonProperty("orderid")]
        public string OrderId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }
    }

}
