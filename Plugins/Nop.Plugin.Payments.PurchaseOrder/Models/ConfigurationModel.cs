using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.PurchaseOrder.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel, IAclSupportedModel
    {
        public ConfigurationModel()
        {
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.ShippableProductRequired")]
        public bool ShippableProductRequired { get; set; }

        public bool ShippableProductRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.ShowImpersonated")]
        public bool ShowImpersonated { get; set; }
        public bool ShowImpersonated_OverrideForStore { get; set; }        

        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.LimitedToRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public bool SelectedCustomerRoleIds_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }        
    }
}