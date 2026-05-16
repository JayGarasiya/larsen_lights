using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Inventory.Order.Components;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using System.Text;

namespace Nop.Plugin.Misc.Inventory.Order
{
    /// <summary>
    /// Represents inventory order misc
    /// </summary>
    public class InventoryOrderPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        #region Fields
        protected readonly ILocalizationService _localizationService;
        protected readonly INopDataProvider _dataProvider;
        protected readonly INopFileProvider _fileProvider;
        protected readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public InventoryOrderPlugin(ILocalizationService localizationService,
            INopDataProvider dataProvider,
            INopFileProvider fileProvider,
            ISettingService settingService)
        {
            _localizationService = localizationService;
            _dataProvider = dataProvider;
            _fileProvider = fileProvider;
            _settingService = settingService;
        }
        #endregion

        #region Methods

        #region Widget

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.ProductDetailsBlock, InventoryOrderDefault.ProductDetailsAvailability });
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
                return typeof(ProductDimensionViewComponent);

            if (widgetZone.Equals(InventoryOrderDefault.ProductDetailsAvailability))
                return typeof(ProductAvailabilityDetails);

            return null;
        }

        #endregion

        #region Install/Uninstall
        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            // locals
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.InventoryOrder.Fields.PluginsTitle"] = "Plugins",
                ["Plugins.InventoryOrder.Fields.NopCommercePlusTitle"] = "NopCommerce Plus",
                ["Plugins.InventoryOrder.Fields.Configuration"] = "Configuration",
                ["Plugins.InventoryOrder.Fields.AllowedManufacturers"] = "Allowed manufacturers",
                ["Plugins.InventoryOrder.Fields.AllowedManufacturers.Hint"] = "Select manufacturers to allow in PO Order.",
                ["Plugins.InventoryOrder.Fields.Category"] = "Category",
                ["Plugins.InventoryOrder.Fields.Category.Hint"] = "Please Select Category",
                ["Plugins.InventoryOrder.Fields.Manufacture"] = "Manufacture",
                ["Plugins.InventoryOrder.Fields.Manufacture.Hint"] = "Please Select Manufacture",
                ["Plugins.InventoryOrder.Fields.SearchIncreOrDecre"] = "% increase or decrease",
                ["Plugins.InventoryOrder.Fields.SearchIncreOrDecre.Hint"] = "Search % increase or decrease",
                ["Plugins.InventoryOrder.Fields.StartDate"] = "Start Date",
                ["Plugins.InventoryOrder.Fields.StartDate.Hint"] = "Please select start date",
                ["Plugins.InventoryOrder.Fields.EndDate"] = "End Date",
                ["Plugins.InventoryOrder.Fields.EndDate.Hint"] = "Please select end date",
                ["Plugins.InventoryOrder.Fields.TotalBoxVolume"] = "Total Order Volume",
                ["Plugins.InventoryOrder.Fields.TotalBoxVolume.Hint"] = "Total Order Volume",
                ["Plugins.InventoryOrder.Fields.Manufacture"] = "Manufacture",
                ["Plugins.InventoryOrder.Fields.SKU"] = "SKU",
                ["Plugins.InventoryOrder.Fields.Name"] = "Name",
                ["Plugins.InventoryOrder.Fields.PictureThumbnailUrl"] = "Picture",
                ["Plugins.InventoryOrder.Fields.InStock"] = "In stock",
                ["Plugins.InventoryOrder.Fields.TimeFrameQty"] = "QTY Needed for TimeFrame selected",
                ["Plugins.InventoryOrder.Fields.IncOrDesQty"] = "QTY % increase or decrease total",
                ["Plugins.InventoryOrder.Fields.ItemQty1"] = "Container 1",
                ["Plugins.InventoryOrder.Fields.ItemQty2"] = "Container 2",
                ["Plugins.InventoryOrder.Fields.ItemQty3"] = "Container 3",
                ["Plugins.InventoryOrder.Fields.ItemQty4"] = "Container 4",
                ["plugins.inventoryorder.fields.totalqty"] = "Total Order Qty",
                ["Plugins.InventoryOrder.Fields.TotalCartoon"] = "Total Cartoon",
                ["Plugins.InventoryOrder.Fields.BoxVolume"] = "Box Volume",
                ["Plugins.InventoryOrder.Fields.ProductCost"] = "Product cost",
                ["Plugins.InventoryOrder.Fields.PONumber"] = "PONumber",
                ["Plugins.InventoryOrder.Fields.PONumber.Hint"] = "Search a ponumber",
                ["Plugins.InventoryOrder.Fields.Comment"] = "Admin Comment",
                ["Plugins.InventoryOrder.Fields.Comment.Hint"] = "Please enter a Comment",
                ["Plugins.InventoryOrder.Fields.HasRecived"] = "HasRecived",
                ["Plugins.InventoryOrder.Fields.PercentAdjusted"] = "PercentAdjusted",
                ["Plugins.InventoryOrder.Fields.CreatedOnUTC"] = "Created on",
                ["Plugins.InventoryOrder.Fields.ReceivedOnUTC"] = "Received On",
                ["Plugins.InventoryOrder.Fields.ManufacturerName"] = "Manufacturer Name",
                ["Plugins.InventoryOrder.Fields.ProductName"] = "Product Name",
                ["Plugins.InventoryOrder.Fields.orderedqty"] = "Order Qty",
                ["Plugins.InventoryOrder.Fields.orderedqty.Hint"] = "Enter a order qty",
                ["Plugins.InventoryOrder.Fields.SearchPoNumber"] = "PoNumber",
                ["Plugins.InventoryOrder.Fields.SearchPoNumber.Hint"] = "Search a ponumber",
                ["Plugins.InventoryOrder.Fields.SearchAdminComment"] = "Admin Comment",
                ["Plugins.InventoryOrder.Fields.SearchAdminComment.Hint"] = "Search a admincomment",
                ["Plugins.InventoryOrder.Fields.DownloadPDF"] = "Download PDF",
                ["Plugins.InventoryOrder.Fields.Title"] = "Manage Inventory",
                ["Plugins.InventoryOrder.Fields.ManageInventoryItemsTitle"] = "Manage Inventory Items",
                ["Plugins.InventoryOrder.Fields.GeneratePoOrder"] = "Generate PoOrder",
                ["Plugins.InventoryOrder.Fields.QtyCartoon"] = "Qty Cartoon",
                ["Plugins.InventoryOrder.Fields.QtyCartoon.Hint"] = "Enter a Qty Cartoon",
                ["Plugins.InventoryOrder.Fields.DimensionsHeight"] = "Height",
                ["Plugins.InventoryOrder.Fields.DimensionsHeight.Hint"] = "Enter a Height",
                ["Plugins.InventoryOrder.Fields.DimensionsLength"] = "Length",
                ["Plugins.InventoryOrder.Fields.DimensionsLength.Hint"] = "Enter a Length",
                ["Plugins.InventoryOrder.Fields.DimensionsWidth"] = "Width",
                ["Plugins.InventoryOrder.Fields.DimensionsWidth.Hint"] = "Enter a Width",
                ["Plugins.InventoryOrder.Fields.ProductDimensionsTitle"] = "Product Dimensions",
                ["Plugins.InventoryOrder.Fields.SaveProductDimesions"] = "Save",
                ["Plugins.InventoryOrder.Fields.Edit"] = "Edit Order Qty",
                ["Plugins.InventoryOrder.Fields.AddNew"] = "Add New Comment",
                ["Plugins.InventoryOrder.Fields.Received"] = "Received",
                ["Plugins.InventoryOrder.Fields.Unreceived"] = "Unreceived",
                ["Plugins.InventoryOrder.Fields.PoOrderTitle"] = "Po-Order",
                ["Plugins.InventoryOrder.Fields.CreatePoOrderTitle"] = "Create Po-Order",
                ["Plugins.InventoryOrder.Fields.ManagePoOrderItemsTitle"] = "Manage Po-Order Items",
                ["Plugins.InventoryOrder.Product.Availability.Message"] = "More {0} quantity will be available by {1}.",
                ["Plugins.InventoryOrder.Product.Availability.Title"] = "Product Availability",
                ["Plugins.Inventoryorder.Fields.AvailabledateOnUtc"] = "Available Date",
                ["Plugins.Inventoryorder.Fields.AvailabledateOnUtc.Hint"] = "Enter a date when all products are available.",
                ["Plugins.InventoryOrder.Fields.EditPoOrderTitle"] = "Edit Po-Order Detalis",
                ["Plugins.InventoryOrder.Fields.PoOrder.Edit"] = "Add items in PO Order - {0}",
                ["Plugins.InventoryOrder.Fields.PoOrder.AddItem"] = "Save items",
                ["Plugins.InventoryOrder.Fields.AddItems"] = "Add items",
                ["Plugins.InventoryOrder.Fields.TotalVolume"] = "Total Volume",
                ["poorderitem.manufacturer"] = "Manufacturer",
                ["poorderitem.productname"] = "Product name",
                ["poorderitem.sku"] = "SKU",
                ["poorderitem.productcost"] = "Product Cost",
                ["poorderitem.qty"] = "QTY",
                ["Pdf.OrderComment"] = "Comment",
                ["Pdf.TotalVolume"] = "Total Volume",
                ["Pdf.PoOrder"] = "PO Number",
                ["Plugins.InventoryOrder.Fields.Received.AdditionalConfirm"] = "Are you sure you want to mark as unreceived this item?",
                ["Plugins.InventoryOrder.Fields.Unreceived.AdditionalConfirm"] = "Are you sure you want to mark as received this item?",
                ["Plugins.InventoryOrder.PoOrder.NoPoOrders"] = "No PoNumber selected",
                ["Plugins.InventoryOrder.PoOrder.PdfPoInvoices"] = "Print PDFs",
                ["Plugins.InventoryOrder.PoOrder.PdfPoInvoices.All"] = "Print PDFs (all found)",
                ["Plugins.InventoryOrder.PoOrder.PdfPoInvoices.Selected"] = "Print PDFs (selected)",
                ["Plugins.Misc.OrderQty.AddNew"] = "Add new po Order",
            });

            // sp
            await CreateManageInventorySP();

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<InventoryOrderSettings>();

            // locals
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.InventoryOrder.Fields");
            await _localizationService.DeleteLocaleResourcesAsync("poorderitem");

            // sp
            await RemoveManageInventorySP();

            await base.UninstallAsync();
        }
        #endregion

        #region ManageInventorySP

        #region Create SP 
        public async Task CreateManageInventorySP()
        {
            var robotsFilePath = _fileProvider.Combine("Plugins/Inventory.Order/SqlScripts/ManageInventorySP.sql");

            var existingText = await _fileProvider.ReadAllTextAsync(robotsFilePath, Encoding.UTF8);

            _ = await _dataProvider.ExecuteNonQueryAsync(existingText);
        }
        #endregion

        #region DROP SP 
        public async Task RemoveManageInventorySP()
        {
            await _dataProvider.ExecuteNonQueryAsync("DROP PROCEDURE [dbo].[ManageInventorySP]");
        }
        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        #endregion
    }
}
