using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Nop.Web.Areas.Admin.Models.Customers;

namespace Nop.Plugin.Misc.InvoicePDF.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            CustomerSearchModel = new CustomerSearchModel();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }
        
        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.AttachOrderId")]
        public bool AttachOrderId { get; set; }
        public bool AttachOrderId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.UseOrderMask")]
        public bool UseOrderMask { get; set; }
        public bool UseOrderMask_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.VendorCost")]
        public bool VendorCost { get; set; }
        public bool VendorCost_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.RenderAttributeValuePrices")]
        public bool RenderAttributeValuePrices { get; set; }
        public bool RenderAttributeValuePrices_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.RenderAssociatedAttributeValueQuantity")]
        public bool RenderAssociatedAttributeValueQuantity { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.ReturnAddress")]
        public string ReturnAddress { get; set; }
        public bool ReturnAddress_OverrideForStore { get; set; }

        public CustomerSearchModel CustomerSearchModel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.InvoicePDF.Customer")]
        public int CustomerId { get; set; }

        #endregion
    }
}