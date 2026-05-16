using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            MultiPackageForCountries = new List<int>();
            AvailableCountries = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlEnable")]
        public bool ThreePlEnable { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ClientId")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ClientSecret")]
        public string ClientSecret { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlKey")]
        public string ThreePlKey { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.UserId")]
        public string UserId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.CustomerId")]
        public string CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.FacilityId")]
        public string FacilityId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.SendOrderHoursInterval")]
        public int SendOrderHoursInterval { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.MultiPackageForCountries")]
        public IList<int> MultiPackageForCountries { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.MultiPackageOrderAmountOver")]
        public decimal MultiPackageOrderAmountOver { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.NoSplitAmountLess")]
        public decimal NoSplitAmountLess { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public bool ValidateCredentials { get; set; }

        #endregion
    }
}
