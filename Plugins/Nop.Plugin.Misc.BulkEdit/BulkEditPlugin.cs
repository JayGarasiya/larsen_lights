using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.BulkEdit;

/// <summary>
/// Represents the Bulk Edit Plugin
/// </summary>
public class BulkEditPlugin : BasePlugin, IMiscPlugin
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public BulkEditPlugin(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        // Adding or updating locale resources specific to the BulkEdit plugin
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.BulkEdit.Title"] = "Bulk Edit Products",
            ["Plugins.Misc.BulkEdit.List.SearchProductName"] = "Product name",
            ["Plugins.Misc.BulkEdit.List.SearchProductName.Hint"] = "A product name.",
            ["Plugins.Misc.BulkEdit.List.SearchCategory"] = "Category",
            ["Plugins.Misc.BulkEdit.List.SearchCategory.Hint"] = "Search by a specific category.",
            ["Plugins.Misc.BulkEdit.List.SearchManufacturer"] = "Manufacturer",
            ["Plugins.Misc.BulkEdit.List.SearchManufacturer.Hint"] = "Search by a specific manufacturer.",
            ["Plugins.Misc.BulkEdit.List.SearchProductType"] = "Product type",
            ["Plugins.Misc.BulkEdit.List.SearchProductType.Hint"] = "Search by a product type.",
            ["Plugins.Misc.BulkEdit.Fields.PictureThumbnailUrl"] = "Picture",
            ["Plugins.Misc.BulkEdit.Fields.Name"] = "Name",
            ["Plugins.Misc.BulkEdit.Fields.OldPrice"] = "Old price",
            ["Plugins.Misc.BulkEdit.Fields.Price"] = "Price",
            ["Plugins.Misc.BulkEdit.Fields.ProductCost"] = "Product cost",
            ["Plugins.Misc.BulkEdit.Fields.Published"] = "Published",
            ["Plugins.Misc.BulkEdit.Fields.SKU"] = "SKU",
            ["Plugins.Misc.BulkEdit.Fields.StockQuantity"] = "Stock qty",
            ["Plugins.Misc.BulkEdit.Fields.ManageInventoryMethod"] = "Manage inventory",
            ["Plugins.Misc.BulkEdit.Pager.Display"] = "{0} - {1} of {2} items",
            ["Plugins.Misc.BulkEdit.Pager.Empty"] = "No items to display",
            ["Plugins.Misc.BulkEdit.Pager.Page"] = "Page",
            ["Plugins.Misc.BulkEdit.Pager.Of"] = "of {0}",
            ["Plugins.Misc.BulkEdit.Pager.ItemsPerPage"] = "items per page",
            ["Plugins.Misc.BulkEdit.Pager.First"] = "Go to the first page",
            ["Plugins.Misc.BulkEdit.Pager.Previous"] = "Go to the previous page",
            ["Plugins.Misc.BulkEdit.Pager.Next"] = "Go to the next page",
            ["Plugins.Misc.BulkEdit.Pager.Last"] = "Go to the last page",
            ["Plugins.Misc.BulkEdit.Pager.Refresh"] = "Refresh",
            ["Plugins.Misc.BulkEdit.Pager.All"] = "All",
            ["Plugins.Misc.BulkEdit.Pager.MorePages"] = "More pages"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Deleting all locale resources related to the BulkEdit plugin
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.BulkEdit");

        await base.UninstallAsync();
    }
    #endregion
}
