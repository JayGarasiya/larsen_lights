using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data
{
    [NopMigration("2026/02/01 15:11:07:6455407", "Widgets.MakeTypeModel", MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IMessageTemplateService _messageTemplateService;
        #endregion

        #region Ctor
        public DataMigration(ILocalizationService localizationService, IScheduleTaskService scheduleTaskService, INopDataProvider nopDataProvider, IMessageTemplateService messageTemplateService)
        {
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _nopDataProvider = nopDataProvider;
            _messageTemplateService = messageTemplateService;
        }
        #endregion

        #region Methods

        public override async void Up()
        {
            var correctTable = nameof(GranitProductImport);
            var oldTable = "GraniteProductImport";

            if (!Schema.Table(correctTable).Exists())
            {
                if (Schema.Table(oldTable).Exists())
                {
                    Rename.Table(oldTable).To(correctTable);
                }
                else
                {
                    Create.Table(correctTable);
                }
            }

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.MakeTypeModel.Navigation.GranitProductImports"] = "Granit imports",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitCategoryId"] = "Granit category",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitCategoryId.Hint"] = "Select a granit category for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitVendorId"] = "Granit vendor",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitVendorId.Hint"] = "Select a granit vendor for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitWarehouseId"] = "Granit warehouse",
                ["Plugins.Widgets.MakeTypeModel.Fields.GranitWarehouseId.Hint"] = "Select a granit warehouse for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.UpdateProductImage"] = "Update Product images",
                ["Plugins.Widgets.MakeTypeModel.Fields.UpdateProductImage.Hint"] = "Check to update Product images on granit import",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.SearchImportStatus"] = "Import status",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.SearchImportStatus.Hint"] = "search using status of importing data.",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.FileName"] = "File name",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.DataRowNumber"] = "No of data sheet rows processed",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.ImportStatus"] = "Import status",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.CreatedOnUtc"] = "Created on",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.Imported"] = "Products have been imported to queue successfully.",
                ["Plugins.Widgets.MakeTypeModel.GranitProductImport.ImportedExist"] = "There is already imported file in queue. Please upload another file after previous one is complete.",
                ["Plugins.Widgets.MakeTypeModel.ReturnRequest.Fields.Select"] = "Select",
                ["Plugins.Widgets.MakeTypeModel.ReturnRequest.Fields.Name"] = "Name",
                ["Plugins.Widgets.MakeTypeModel.ReturnRequest.Fields.Quantity"] = "Quantity",
                ["Plugins.Widgets.MakeTypeModel.ReturnRequest.Fields.Price"] = "Price",

                // enum
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportStatusEnum.Pending"] = "Pending",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportStatusEnum.Processing"] = "Processing",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportStatusEnum.Complete"] = "Complete",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportTypeEnum.Products"] = "Products",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportTypeEnum.ModelFit"] = "ModelFit",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.GraniteProductImportTypeEnum.Stock"] = "Stock",

                //Price import
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.SearchImportType"] = "Import Type",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.SearchImportType.Hint"] = "search using type of importing data.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.SearchImportStatus"] = "Import status",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.SearchImportStatus.Hint"] = "search using status of importing data.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.UsePercentage"] = "Use Percentage",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.UsePercentage.Hint"] = "Check use percentage for enter price percentage.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.PricePercentage"] = "Price Percentage",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.PricePercentage.Hint"] = "Enter price percentage.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.PriceAmount"] = "Price Amount",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.PriceAmount.Hint"] = "Enter price amount.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.FromPrice"] = "From Price",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.FromPrice.Hint"] = "Enter from price.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ToPrice"] = "To Price",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ToPrice.Hint"] = "Enter to price.",
                ["Plugins.Widgets.MakeTypeModel.Navigation.PriceImports"] = "Price imports",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.DeleteAll"] = "Delete / Unpublish all",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.DeleteAll.Hint"] = "Check to delete / unpublish all existing map with importing data.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ImportType"] = "Import Type",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ImportType.Hint"] = "type of importing data.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.FileName"] = "File Name",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ImportStatus"] = "Import Status",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.RowNumber"] = "Row Number",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.CreatedOnUtc"] = "Created OnUtc",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Imported"] = "Products price have been imported to queue successfully.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.ImportedExist"] = "Imported file with same name already in queue. Please change file name and import again.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.IsMulitplePriceRange"] = "Is Mulitple Price Range",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.IsMulitplePriceRange.Hint"] = "Check to enter mulitple price range.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.PriceValue"] = "Price Value",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ExcelFile"] = "Import File",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ExcelFile.Hint"] = "Upload the excel file for the price import.",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Vendor.Required"] = "Please select any one vendor.",
                ["Plugins.Widgets.MakeTypeModel.PriceImports.BackToList"] = "Back to price import list",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.VendorId"] = "Vendor",
                ["Plugins.Widgets.MakeTypeModel.PriceImport.Fields.VendorId.Hint"] = "Select a Vendor",
                ["Plugins.Widgets.MakeTypeModel.PriceImports.BackToList"] = "Back to price import list",
                ["Plugins.Widgets.MakeTypeModel.Navigation.PriceImports.Add"] = "Add a new Price import",
                ["Plugins.Widgets.MakeTypeModel.Navigation.PriceImports.Update"] = "Update Price import",

                //config
                ["Plugins.Widgets.MakeTypeModel.Fields.AuthorizationAPIURL"] = "Authorization API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.AuthorizationAPIURL.Hint"] = "Enter Authorization API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductsAPIURL"] = "Products API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductsAPIURL.Hint"] = "Enter Products API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.InventoryAPIURL"] = "Inventory API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.InventoryAPIURL.Hint"] = "Enter Inventory API URL",
                ["Plugins.Widgets.MakeTypeModel.Fields.Username"] = "User Name",
                ["Plugins.Widgets.MakeTypeModel.Fields.Username.Hint"] = "Enter User Name",
                ["Plugins.Widgets.MakeTypeModel.Fields.Password"] = "Password",
                ["Plugins.Widgets.MakeTypeModel.Fields.Password.Hint"] = "Enter Password",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPIPagesize"] = "Product update API page size",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPIPagesize.Hint"] = "Product update API page size",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPICurrentPage"] = "Product update API current page",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPICurrentPage.Hint"] = "Product update API current page",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPINoOfPages"] = "Product update API No of pages",
                ["Plugins.Widgets.MakeTypeModel.Fields.ProductUpdateAPINoOfPages.Hint"] = "Product update API No of pages",
                ["Plugins.Widgets.MakeTypeModel.Fields.LastExecutedProductAPI"] = "Last Product API Executed Date",
                ["Plugins.Widgets.MakeTypeModel.Fields.LastExecutedProductAPI.Hint"] = "Last Product API Executed Date",
                ["Plugins.Widgets.MakeTypeModel.Fields.LastExecutedStockAPI"] = "Last Stock API Executed Date",
                ["Plugins.Widgets.MakeTypeModel.Fields.LastExecutedStockAPI.Hint"] = "Last Stock API Executed Date",
                ["Plugins.Widgets.MakeTypeModel.Hy-CapacityAPI"] = "Hy-Capacity API",
                ["Plugins.Widgets.MakeTypeModel.CommonSettings"] = "Common Settings",
                ["Plugins.Widgets.MakeTypeModel.Fields.TokenGuidId"] = "Token Guid Id",
                ["Plugins.Widgets.MakeTypeModel.Fields.TokenGuidId.Hint"] = "Enter Token Guid Id",
                ["Plugins.Widgets.MakeTypeModel.SKU"] = "SKU",
                ["Plugins.Widgets.MakeTypeModel.SKU.Hint"] = "Product information via product SKU",
                ["Plugins.Widgets.MakeTypeModel.Note"] = "Note For Email",
                ["Plugins.Widgets.MakeTypeModel.Note.Hint"] = "Enter Note For Email.",
                ["Plugins.Widgets.MakeTypeModel.Fields.ExcludeCategory"] = "Exclude Category",
                ["Plugins.Widgets.MakeTypeModel.Fields.ExcludeCategory.Hint"] = "Enter Exclude Category.",

                ["Plugins.Widgets.MakeTypeModel.Fields.EnabledFreeSkus"] = "Enable Free SKUs",
                ["Plugins.Widgets.MakeTypeModel.Fields.EnabledFreeSkus.Hint"] = "Check to enable free SKUs",
                ["Plugins.Widgets.MakeTypeModel.Fields.SubTotalGreaterThan"] = "SubTotal Greater Than",
                ["Plugins.Widgets.MakeTypeModel.Fields.SubTotalGreaterThan.Hint"] = "Enter sub total amount for which free sku should be add",
                ["Plugins.Widgets.MakeTypeModel.Fields.FreeItemsSkus"] = "Free Items Skus",
                ["Plugins.Widgets.MakeTypeModel.Fields.FreeItemsSkus.Hint"] = "Enter free item skus",
                ["Plugins.Widgets.MakeTypeModel.FreeItemSku"] = "Free Item Configuration",
                ["Plugins.Widgets.MakeTypeModel.Fields.FreeItemsWarehouseIds"] = "Free Item Warehouse Configuration",
                ["Plugins.Widgets.MakeTypeModel.Fields.FreeItemsWarehouseIds.Hint"] = "Select warehouse for free item should be add",
            });
        }

        public override void Down()
        {
        }
        #endregion
    }
}
