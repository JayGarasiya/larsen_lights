using Nop.Core;
using Nop.Plugin.Widgets.ProductExtension.Components;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.ProductExtension
{
    /// <summary>
    /// Represents the Product extension plugin
    /// </summary>
    public class ProductExtensionPlugin : BasePlugin, IWidgetPlugin
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;
        protected readonly IProductExtendService _productExtendService;
        protected readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public ProductExtensionPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            IProductExtendService productExtendService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _productExtendService = productExtendService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgets = new List<string>() {
                ProductExtensionDefaults.ProductDetailsAfterCollateral,
                PublicWidgetZones.OrderSummaryCartFooter,
                PublicWidgetZones.HeaderSelectors,
                PublicWidgetZones.OrderSummaryTotals
            };
            var noteWidgets = await _productExtendService.GetAllProductNoteWidgetsAsync();
            if (noteWidgets.Any())
                widgets.AddRange(noteWidgets);

            return widgets;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ProductExtension/Configure";
        }

        /// <summary>
        /// Gets a type of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            ArgumentNullException.ThrowIfNull(widgetZone);

            if (widgetZone.Equals(ProductExtensionDefaults.ProductDetailsAfterCollateral))
                return typeof(ProductReviewsViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.OrderSummaryCartFooter))
                return typeof(CartQuoteViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.HeaderSelectors))
                return typeof(DealerPriceSelectorViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.OrderSummaryTotals))
                return typeof(DealerPriceConfirmationViewComponent);

            return typeof(ProductNoteViewComponent);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locals    
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.ProductExtension.Fields.Enable"] = "Enable",
                ["Plugins.Widgets.ProductExtension.Fields.Enable.Hint"] = "Check to allow customers to use the 'Product extension' feature in your store.",

                ["Plugins.Widgets.ProductExtension.Fields.DealerRoleIds"] = "Dealer roles",
                ["Plugins.Widgets.ProductExtension.Fields.DealerRoleIds.Hint"] = "Select customer roles to determines whether the customer is dealer.",

                ["Plugins.Widgets.ProductExtension.Fields.Dealer.Prices"] = "Prices",
                ["Plugins.Widgets.ProductExtension.Fields.Dealer.DealerPricing"] = "Dealer pricing",
                ["Plugins.Widgets.ProductExtension.Fields.Dealer.ListPricing"] = "List pricing",
                ["Plugins.Widgets.ProductExtension.Fields.Dealer.AllPrices"] = "All prices {0}",

                ["Plugins.Widgets.ProductExtension.DealerPriceConfirmation.Title"] = "Are you sure you want to process with list price?",
                ["Plugins.Widgets.ProductExtension.DealerPriceConfirmation.Body"] = @"If you like to process order with dealer price then please change it instead of list price.  <br /><br /> Make sure before continue checkout. <br /><br />",
                ["Plugins.Widgets.ProductExtension.DealerPriceConfirmation.Yes"] = "Yes, Process with list price",
                ["Plugins.Widgets.ProductExtension.DealerPriceConfirmation.No"] = "No",

                ["Plugins.Widgets.ProductExtension.ProductNote.Title"] = "Manage product notes",

                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Name"] = "Display name",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Name.Hint"] = "This is the name of the product note as it will be displayed in the public store front as note title.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Name.Required"] = "Please provide a display name",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Description"] = "Description",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Description.Hint"] = "This is the actuall content of the product note as it will appear in the public store front.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Description.Required"] = "Please provide a description",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.WidgetZone"] = "Widget zone",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.WidgetZone.Hint"] = "Select widget zone where you like to show product note in public store.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.WidgetZone.Required"] = "Please select any widget zone",

                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.DisplayOrder.Hint"] = "The display order of this product note. 1 represents the top of the list.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Published"] = "Published",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.Published.Hint"] = "Check to publish this product note (visible in store). Uncheck to unpublish (product note not available in store).",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.ShowDisplayName"] = "Show display name",
                ["Plugins.Widgets.ProductExtension.ProductNote.Fields.ShowDisplayName.Hint"] = "Check to show note title in the public store front.",

                ["Plugins.Widgets.ProductExtension.ProductNote.Added"] = "	The new product note has been added successfully.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Updated"] = "The product note has been updated successfully.",
                ["Plugins.Widgets.ProductExtension.ProductNote.Deleted"] = "The product note has been deleted successfully.",
                ["Plugins.Widgets.ProductExtension.ProductNote.EditProductNoteDetails"] = "Edit product note details",
                ["Plugins.Widgets.ProductExtension.ProductNote.BackToList"] = "back to product note list",
                ["Plugins.Widgets.ProductExtension.ProductNote.AddNew"] = "	Add a new product note",
                ["Plugins.Widgets.ProductExtension.ProductNote.Added"] = "	The new product note has been added successfully.",

                ["Plugins.Widgets.ProductExtension.ImportPriceFromExcel"] = "Import price from Excel",
                ["Plugins.Widgets.ProductExtension.ImportPriceFromExcelTip"] = "Imported products price are distinguished by SKU. If the SKU exists, then its corresponding product price will be updated.",
                ["Plugins.Widgets.ProductExtension.ProductPrice.Imported"] = "Products price have been imported successfully.",

                ["Plugins.Widgets.ProductExtension.ImportStockFromExcel"] = "Import stock from Excel",
                ["Plugins.Widgets.ProductExtension.ImportStockFromExcelTip"] = "Imported products stock are distinguished by SKU. If the SKU exists, then its corresponding product price will be updated.",
                ["Plugins.Widgets.ProductExtension.ProductStock.Imported"] = "Products stock have been imported successfully.",

                ["Plugins.Widgets.ProductExtension.ProductAttributeValue.Condition"] = "Condition",
                ["Plugins.Widgets.ProductExtension.ProductAttributeValue.ConditionString"] = "Then below options should be pre-selected: <br /> {0}",
                ["Plugins.Widgets.ProductExtension.ProductAttributeValue.ConditionDescription"] = "When <b>{0} : {1}</b> option selected, then below options should be pre-selected.",

                ["Plugins.Widgets.ProductExtension.PDFQuote.Button"] = "Cart PDF",
                ["Plugins.Widgets.ProductExtension.PDFQuote.Cart#"] = "Cart# {0}",

                ["Plugins.Widgets.ProductExtension.Products.Fields.TotalPrice"] = "Total price",
                ["Plugins.Widgets.ProductExtension.Products.Fields.TotalProductCost"] = "Total product cost",

                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.DownloadPDF"] = "Download as PDF",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.DownloadStockPDF"] = "Download stock as PDF",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.CreatedOn"] = "Date",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.Stock"] = "Stock",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.ProductCost"] = "Cost",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalProductCost"] = "Total cost",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.PicturePath"] = "Picture",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalCost"] = "Total cost",
                ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalPrice"] = "Total price",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchFromWeight"] = "From weight",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchFromWeight.Hint"] = "Search by From weight",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchToWeight"] = "To weight",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchToWeight.Hint"] = "Search by To weight",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchNoPicture"] = "No Picture",
                ["Plugins.Widgets.ProductExtension.Products.List.SearchNoPicture.Hint"] = "Search products which doesn't contain any picture.",
            });

            await _localizationService.AddOrUpdateLocaleResourceAsync("Products.Availability.OutOfStock", @"<span class=""stock-error"">Out of stock</span>");

            await base.InstallAsync();
        }

        /// <summary>
        /// Update plugin
        /// </summary>
        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (!targetVersion.Equals("2.00"))
            {
                //locales
                await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
                {
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchFromWeight"] = "From weight",
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchFromWeight.Hint"] = "Search by From weight",
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchToWeight"] = "To weight",
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchToWeight.Hint"] = "Search by To weight",
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchNoPicture"] = "No Picture",
                    ["Plugins.Widgets.ProductExtension.Products.List.SearchNoPicture.Hint"] = "Search products which doesn't contain any picture.",

                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.DownloadPDF"] = "Download as PDF",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.DownloadStockPDF"] = "Download stock as PDF",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.CreatedOn"] = "Date",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.Stock"] = "Stock",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.ProductCost"] = "Cost",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalProductCost"] = "Total cost",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.PicturePath"] = "Picture",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalCost"] = "Total cost",
                    ["Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalPrice"] = "Total price",
                });
            }
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.ProductExtension");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Products.Availability.OutOfStock", "Out of stock");

            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Widgets.ProductExtension.CatalogStockPdf");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        #endregion

    }
}