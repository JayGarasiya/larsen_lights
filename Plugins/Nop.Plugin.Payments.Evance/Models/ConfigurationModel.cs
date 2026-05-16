using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Evance.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            AvailablePaymentOptionIds = new List<SelectListItem>();
            AvailableTransactionTypesIds = new List<SelectListItem>();
            AvailableExcludeCustomerRoleTds = new List<SelectListItem>();
            ExcludeCustomerRoleIds = new List<int>();
        }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.Enable")]
        public bool Enable { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.ThreedSEnable")]
        public bool ThreedSEnable { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.ExcludeCustomerRoleIds")]
        public IList<int> ExcludeCustomerRoleIds { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.CustomerVaultEnable")]
        public bool CustomerVaultEnable { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.AllowSaveCard")]
        public bool AllowSaveCard { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.AllowSaveACH")]
        public bool AllowSaveACH { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.DisplaySavedDetails")]
        public bool DisplaySavedDetails { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.SecurityKey")]
        public string SecurityKey { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.PaymentAPIBaseUrl")]
        public string PaymentAPIBaseUrl { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.QueryAPIBaseUrl")]
        public string QueryAPIBaseUrl { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.CollectCheckoutKey")]
        public string CollectCheckoutKey { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.Payments")]
        public IList<int> PaymentOptionIds { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.TransactionType")]
        public int TransactionTypeId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Evance.Fields.DropTable")]
        public bool DropTable { get; set; }
        public IList<SelectListItem> AvailablePaymentOptionIds { get; set; }
        public IList<SelectListItem> AvailableTransactionTypesIds { get; set; }
        public IList<SelectListItem> AvailableExcludeCustomerRoleTds { get; set; }
    }
}
