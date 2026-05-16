using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Evance.Models
{
    /// <summary>
    /// Represents a payment info model
    /// </summary>
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            CardInformation = new List<CustomerCardInfo>();
            ACHInformation = new List<CustomerACHInfo>();
        }
        public bool IsPaymentOptions { get; set; }
        public bool AllowSaveCard { get; set; }
        public bool AllowSaveACH { get; set; }
        public string ThreedSJSEnable { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Email { get; set; }
        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string BillingPhoneNumber { get; set; }
        public string BillingCity { get; set; }
        public string BillingZip { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCountryCode { get; set; }
        public string BillingStateAbbreviation { get; set; }
        public string CardholderName { get; set; }
        public string PaymentToken { get; set; }
        public string CollectCheckoutKey { get; set; }

        public List<int> SelectedPaymentOptionIds { get; set; }
        public List<PaymentMethodModel> PaymentMethods { get; set; }
        public List<CustomerCardInfo> CardInformation { get; set; }
        public List<CustomerACHInfo> ACHInformation { get; set; }

    }
    public class CustomerCardInfo {
        public string CustomerCardNumber { get; set; }
        public string CustomerExpirationDate { get; set; }
        public string CardType { get; set; }
        public string CustomerVaultId { get; set; }

    }
    public class CustomerACHInfo
    {
        public string CustomerAccountNumber { get; set; }
        public string CustomerVaultId { get; set; }

    }
}
