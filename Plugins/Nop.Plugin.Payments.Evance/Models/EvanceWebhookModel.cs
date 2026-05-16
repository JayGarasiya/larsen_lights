using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Evance.Models
{
    /// <summary>
    /// Represents an evance webhook wrapper
    /// </summary>
    public class EvanceWebhookWrapper
    {
        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("event_body")]
        public EventBody EventBody { get; set; }
    }

    public class EventBody
    {
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("action")]
        public ActionDetails Action { get; set; }

        [JsonProperty("transaction_ids")]
        public List<string> SettledTransactionIds { get; set; }

    }
    public class ActionDetails
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("action_type")]
        public string ActionType { get; set; }

        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }
    }
}
