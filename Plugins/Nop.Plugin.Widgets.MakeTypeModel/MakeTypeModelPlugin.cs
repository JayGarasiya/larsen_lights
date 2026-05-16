using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Widgets.MakeTypeModel.Components;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.MakeTypeModel
{
    /// <summary>
    /// Represents a make type model plugin
    /// </summary>
    public class MakeTypeModelPlugin : BasePlugin, IWidgetPlugin, ISearchProvider
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly ISettingService _settingService;
        protected readonly IWebHelper _webHelper;
        protected readonly IPermissionService _permissionService;
        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly IMessageTemplateService _messageTemplateService;
        protected readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MakeTypeModelPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            IScheduleTaskService scheduleTaskService,
            IMakeTypeModelService makeTypeModelService,
            IMessageTemplateService messageTemplateService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _scheduleTaskService = scheduleTaskService;
            _makeTypeModelService = makeTypeModelService;
            _messageTemplateService = messageTemplateService;
            _storeContext = storeContext;
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
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> {
                PublicWidgetZones.HeaderMenuAfter,
                AdminWidgetZones.ProductDetailsBlock,
                MakeTypeModelDefaults.ProductDetailsAfterCollateral,
                PublicWidgetZones.CategoryDetailsTop
            });
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/MakeTypeModel/Configure";
        }

        /// <summary>
        /// Gets a type of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone.Equals(PublicWidgetZones.HeaderMenuAfter))
                return typeof(MakeTypeModelViewComponent);
            else if (widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
                return typeof(ProductModelsDetailViewComponent);
            else if (widgetZone.Equals(PublicWidgetZones.CategoryDetailsTop))
                return typeof(FindPartsCategoryViewComponent);
            else
                return typeof(MakeTypeModelDetailViewComponent);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new MakeTypeModelSettings
            {
                WidgetZone = new List<string> { PublicWidgetZones.HeaderMenuAfter, MakeTypeModelDefaults.ProductDetailsAfterCollateral },
                HyCapacityModelFitsRecords = 1000,
                HyCapacityProductsRecords = 500,
                GraintProductsRecords = 250,
                PriceImportRecords = 500,
                UpdateProductImage = false
            });

            //install synchronization task
            if (await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.ProductImportTask") == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 300,
                    Name = "Hy-capacity imports",
                    Type = "Nop.Plugin.Widgets.MakeTypeModel.Tasks.ProductImportTask",
                });
            }

            var granitProductImportTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.GraniteProductImportTask");
            if (granitProductImportTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 600,
                    Name = "Granit imports",
                    Type = "Nop.Plugin.Widgets.MakeTypeModel.Tasks.GraniteProductImportTask",
                });
            }

            //install synchronization price import task
            var priceImportTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.PriceImportTask");
            if (priceImportTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 900,
                    Name = "Price imports",
                    Type = "Nop.Plugin.Widgets.MakeTypeModel.Tasks.PriceImportTask",
                });
            }

            //install synchronization hy-capacity product update api task
            var hyCapProductAPITask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.HyCapProductAPITask");
            if (priceImportTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 300,
                    Name = "Hy-cap product update - API",
                    Type = "Nop.Plugin.Widgets.MakeTypeModel.Tasks.HyCapProductAPITask",
                });
            }

            //install synchronization hy-capacity stock update api task
            var hyCapStockAPITask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.HyCapStockAPITask");
            if (hyCapStockAPITask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 86400,
                    Name = "Hy-cap stock update - API",
                    Type = "Nop.Plugin.Widgets.MakeTypeModel.Tasks.HyCapStockAPITask",
                });
            }

            var messageTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(MakeTypeModelDefaults.ReturnRequestNote)).FirstOrDefault();
            if (messageTemplate == null)
            {
                messageTemplate = new MessageTemplate
                {
                    Name = MakeTypeModelDefaults.ReturnRequestNote,
                    BccEmailAddresses = null,
                    Subject = "Send Return Request Note",
                    EmailAccountId = 1,
                    Body = "",
                    IsActive = true,
                    DelayBeforeSend = null,
                    DelayPeriodId = 0,
                    AttachedDownloadId = 0,
                    LimitedToStores = false
                };
                await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);
            }

            //locales
            await SetupPluginLocaleResourceAsync();

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<MakeTypeModelSettings>();

            //schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.ProductImportTask");
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            var granitProductImportTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Widgets.MakeTypeModel.Tasks.GraniteProductImportTask");
            if (granitProductImportTask != null)
                await _scheduleTaskService.DeleteTaskAsync(granitProductImportTask);

            var messageTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(MakeTypeModelDefaults.ReturnRequestNote)).FirstOrDefault();
            if (messageTemplate != null)
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.MakeTypeModel");
            await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Widgets.MakeTypeModel");

            await base.UninstallAsync();
        }

        #region Search provider 

        /// <summary>
        /// Get products identifiers by the specified keywords
        /// </summary>
        /// <param name="keywords">Keywords</param>
        /// <param name="isLocalized">A value indicating whether to search in localized properties</param>
        /// <returns>The task result contains product identifiers</returns>
        public async Task<List<int>> SearchProductsAsync(string keywords, bool isLocalized)
        {
            return await _makeTypeModelService.SearchProductsAsync(keywords, isLocalized);
        }

        #endregion

        #region Resource

        private async Task SetupPluginLocaleResourceAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.MakeTypeModel.Navigation.Title"] = "Make / Model",
                ["Plugins.Widgets.MakeTypeModel.Navigation.MakeProduct"] = "Makes product",
                ["Plugins.Widgets.MakeTypeModel.Navigation.TypeProduct"] = "Types product",
                ["Plugins.Widgets.MakeTypeModel.Navigation.ModelProduct"] = "Models product",
                ["Plugins.Widgets.MakeTypeModel.Navigation.ModelCategory"] = "Model Categories",
                ["Plugins.Widgets.MakeTypeModel.Navigation.ProductImports"] = "Hy-capacity imports",

                ["Plugins.Widgets.MakeTypeModel.Fields.Enable"] = "Enable",
                ["Plugins.Widgets.MakeTypeModel.Fields.Enable.Hint"] = "Check to allow customers to use the 'Make / Type / Model' feature in your store.",

                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityCategoryId"] = "Hy-capacity category",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityCategoryId.Hint"] = "Select Hy-capacity master category for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityManufacturerId"] = "Hy-capacity manufacturer",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityManufacturerId.Hint"] = "Select Hy-capacity manufacturer for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityVendorId"] = "Hy-capacity vendor",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityVendorId.Hint"] = "Select Hy-capacity vendor for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityWarehouseId"] = "Hy-capacity warehouse",
                ["Plugins.Widgets.MakeTypeModel.Fields.HyCapacityWarehouseId.Hint"] = "Select Hy-capacity warehouse for import data.",
                ["Plugins.Widgets.MakeTypeModel.Fields.KitProductModelCategoryId"] = "Kit product model category",
                ["Plugins.Widgets.MakeTypeModel.Fields.KitProductModelCategoryId.Hint"] = "Select kit product model category for show only kit products with this model category.",
                ["Plugins.Widgets.MakeTypeModel.Fields.KitProductCategoryId"] = "Kit product category",
                ["Plugins.Widgets.MakeTypeModel.Fields.KitProductCategoryId.Hint"] = "Select kit product category for show only kit products with this category.",

                ["Plugins.Widgets.MakeTypeModel.MakeProducts"] = "Makes product",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name"] = "Name",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.Hint"] = "Enter the make name.",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.Required"] = "Please provide a make name.",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.NameAlreadyExists"] = "A make already exists with the name: {0}",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Published"] = "Published",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Published.Hint"] = "Determines whether this make is published and can therefore be selected by visitors to your store.",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.DisplayOrder.Hint"] = "The display order of this make. 1 represents the top of the list.",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.SearchName"] = "Make name",
                ["Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.SearchName.Hint"] = "Search for the name of a make.",

                ["Plugins.Widgets.MakeTypeModel.TypeProducts"] = "Types product",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name"] = "Name",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.Hint"] = "Enter the type name.",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.Required"] = "Please provide a type name.",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.NameAlreadyExists"] = "A type already exists with the name: {0}",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Published"] = "Published",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Published.Hint"] = "Determines whether this type is published and can therefore be selected by visitors to your store.",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.DisplayOrder.Hint"] = "The display order of this type. 1 represents the top of the list.",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.SearchName"] = "Type name",
                ["Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.SearchName.Hint"] = "Search for the name of a type.",

                ["Plugins.Widgets.MakeTypeModel.ModelCategories"] = "Model Categories",
                ["Plugins.Widgets.MakeTypeModel.ModelCategorySync"] = "Sync categories",
                ["Plugins.Widgets.MakeTypeModel.ModelCategories.Sync.Successfully"] = "Model Categories have been synced successfully",

                ["Plugins.Widgets.MakeTypeModel.ModelProducts"] = "Models product",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Name"] = "Name",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Name.Hint"] = "Enter the model name.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Name.Required"] = "Please provide a model name.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Description"] = "Description",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Description.Hint"] = "The description of the model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.MakeName"] = "Make",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.MakeName.Hint"] = "The make of the model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.MakeName.Required"] = "Please select make for a model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.TypeName"] = "Type",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.TypeName.Hint"] = "The type of the model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.TypeName.Required"] = "Please select type for a model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Published"] = "Published",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Published.Hint"] = "Determines whether this model is published and can therefore be selected by visitors to your store.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.DisplayOrder.Hint"] = "The display order of this model. 1 represents the top of the list.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchName"] = "Model name",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchName.Hint"] = "Search for the name of a model.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchInDescription"] = "Search in description",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchInDescription.Hint"] = "Check to search in description.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchMakeName"] = "Make",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchMakeName.Hint"] = "Search by a specific make.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchTypeName"] = "Type",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchTypeName.Hint"] = "Search by a specific type.",

                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Added"] = "The new model product has been added successfully.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.AddNew"] = "Add a new model",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.EditModelProductDetails"] = "Edit model details",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Updated"] = "The model product has been updated successfully.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Deleted"] = "The model product has been deleted successfully.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.BackToList"] = "back to models product list",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Info"] = "Model info",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.ImportFromExcelTip"] = "Imported model products are distinguished by SKU. If the SKU already exists, then its corresponding model product will be updated.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.Imported"] = "Model products have been imported to queue successfully.",
                ["Plugins.Widgets.MakeTypeModel.ModelProduct.HasMoreProducts"] = "Model has mapped with more then one products. if like to update then click on 'Save' (it will impact on all prdocuts which are mapped with this model.) or if like to create new model then click on 'Add new' (it will create new model only for this product).",

                ["Plugins.Widgets.MakeTypeModel.ModelProductMappings"] = "Model products",
                ["Plugins.Widgets.MakeTypeModel.ModelProductMapping.Fields.Product"] = "Product",
                ["Plugins.Widgets.MakeTypeModel.ModelProductMapping.Fields.Product.AddNew"] = "Add a new product",
                ["Plugins.Widgets.MakeTypeModel.ModelProductMapping.Fields.Product.SaveBeforeEdit"] = "You need to save the model before you can add products for this model product page.",

                ["Plugins.Widgets.MakeTypeModel.ProductDetails.Accordion"] = "Use On Below Models (Click to expand)",

                ["Plugins.Widgets.MakeTypeModel.FindParts.Make"] = "Make",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Make.Hint"] = "Make",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Make.Required"] = "Please select make to find parts.",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Type"] = "Equipment type",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Type.Hint"] = "Equipment type",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Model"] = "Model",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Model.Hint"] = "Model",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Category"] = "Categories",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Category.Hint"] = "Categories",

                ["Plugins.Widgets.MakeTypeModel.FindParts.MakeModel"] = "Make / Model",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Title"] = "Select your equipment",
                ["Plugins.Widgets.MakeTypeModel.FindParts.Button"] = "Find Parts",

                ["Plugins.Widgets.MakeTypeModel.ProductImports"] = "Hy-capacity imports",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.ImportType"] = "Import type",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.ImportType.Hint"] = "type of importing data.",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.DeleteAll"] = "Delete / Unpublish all",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.DeleteAll.Hint"] = "Check to delete / unpublish all existing map with importing data.",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.FileName"] = "File name",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.RowNumber"] = "No of rows processed",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.ImportStatus"] = "Import status",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.CreatedOnUtc"] = "Created on",

                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportType"] = "Import type",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportType.Hint"] = "search using type of importing data.",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportStatus"] = "Import status",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportStatus.Hint"] = "search using status of importing data.",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.Imported"] = "Products have been imported to queue successfully.",
                ["Plugins.Widgets.MakeTypeModel.ProductImport.ImportedExist"] = "Imported file with same name already in queue. Please change file name and import again.",

                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportStatusEnum.Pending"] = "Pending",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportStatusEnum.Processing"] = "Processing",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportStatusEnum.Complete"] = "Complete",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportTypeEnum.Products"] = "Products",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportTypeEnum.ModelFit"] = "ModelFit",
                ["Enums.Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImportTypeEnum.Stock"] = "Stock",

                // Granit Product Imports
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

                //Hy-Capacity configurations
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
