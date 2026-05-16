using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.InvoicePDF
{
    /// <summary>
    /// Represents the Invoice PDF plugin
    /// </summary>
    public class InvoicePDFPlugin : BasePlugin, IMiscPlugin
    {
        #region Field
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        #endregion

        #region ctor
        public InvoicePDFPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/InvoicePDF/Configure";
        }
        #endregion

        #region method
        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new InvoicePDFSettings
            {
                AttachOrderId = true,
                UseOrderMask = true,
                VendorCost = true
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.InvoicePDF.AttachOrderId"] = "Attach Order # on invoice name",
                ["Plugins.Misc.InvoicePDF.AttachOrderId.Hint"] = "Enable to attach order # on invoice after name.",
                ["Plugins.Misc.InvoicePDF.UseOrderMask"] = "Use Order Mask as Order #",
                ["Plugins.Misc.InvoicePDF.UseOrderMask.Hint"] = "Enable to use order number mask(custom order number) as order #.",
                ["Plugins.Misc.InvoicePDF.VendorCost"] = "Product cost as unit price",
                ["Plugins.Misc.InvoicePDF.VendorCost.Hint"] = "Enable to show product cost as unit price on vendor notification email.",
                ["Plugins.Misc.InvoicePDF.RenderAttributeValuePrices"] = "Render attribute prices",
                ["Plugins.Misc.InvoicePDF.RenderAttributeValuePrices.Hint"] = "Enable to show product attribute prices on notification email and invoice.",
                ["Plugins.Misc.InvoicePDF.RenderAssociatedAttributeValueQuantity"] = "Render associated attribute quantity",
                ["Plugins.Misc.InvoicePDF.RenderAssociatedAttributeValueQuantity.Hint"] = "Enable to show associated attribute quantity on notification email and invoice.",
                ["Plugins.Misc.InvoicePDF.Copy.Template"] = "Copy template for customer role",
                ["Plugins.Misc.InvoicePDF.Customer.Role"] = "Customer Role",
                ["Plugins.Misc.InvoicePDF.CCAttributeId"] = "CC email attribute",
                ["Plugins.Misc.InvoicePDF.CCAttributeId.Hint"] = "Select attribute to send a copy of the email with any recipient of your choice.",
                ["Plugins.Misc.InvoicePDF.ReturnAddress"] = "Return address",
                ["Plugins.Misc.InvoicePDF.ReturnAddress.Hint"] = "Enter return address to show at invoice header.",
                ["Plugins.Misc.InvoicePDF.TrackingNumber"] = "Tracking number",

                ["Plugins.Misc.InvoicePDF.General"] = "Common",
                ["Plugins.Misc.InvoicePDF.ExcludedCustomers"] = "No printed invoice needed",
                ["Plugins.Misc.InvoicePDF.ExcludedCustomer.CustomerAlreadyExists"] = "A customer already exists with the email: {0}",
                ["Plugins.Misc.InvoicePDF.Customer"] = "Customer",
                ["Plugins.Misc.InvoicePDF.Customer.Hint"] = "Search a specific customer by email address.",

                ["Plugins.Misc.InvoicePDF.PdfInvoices.All"] = "Print Mailable PDF invoices (all)",
                ["Plugins.Misc.InvoicePDF.PdfInvoices.Selected"] = "Print Mailable PDF invoices (selected)",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<InvoicePDFSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.InvoicePDF");

            await base.UninstallAsync();
        }
        #endregion
    }
}