using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Widgets.ProductExtension.Event
{
    /// <summary>
    /// Represents a Admin menu event consumer
    /// </summary>
    public class EventConsumer : IConsumer<AdminMenuCreatedEvent> , IConsumer<EntityDeletedEvent<ProductAttributeValue>>, IConsumer<PageRenderingEvent>
    {
        #region Fields

        protected readonly IPermissionService _permissionService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IAdminMenu _adminMenu;
        protected readonly IProductExtendService _productExtendService;
        protected readonly IPluginService _pluginService;
        protected readonly IActionContextAccessor _actionContextAccessor;
        protected readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public EventConsumer(IPermissionService permissionService,
            ILocalizationService localizationService,
            IAdminMenu adminMenu,
            IProductExtendService productExtendService,
            IPluginService pluginService,
            IActionContextAccessor actionContextAccessor,
            ISettingService settingService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _adminMenu = adminMenu;
            _productExtendService = productExtendService;
            _pluginService = pluginService;
            _actionContextAccessor = actionContextAccessor;
            _settingService = settingService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Handle the sitemap Event
        /// </summary>
        /// <param name="eventMessage">eventMessage</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            {
                var pluginNode = new AdminMenuItem()
                {
                    SystemName = ProductExtensionDefaults.ProductNotesSystemName,
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Title"),
                    Url = _adminMenu.GetMenuItemUrl("ProductExtension", "ProductNotes"),
                    IconClass = "far fa-dot-circle",
                    Visible = true
                };

                var mainMenuNode = eventMessage.RootMenuItem.ChildNodes
                    .FirstOrDefault(x => x.SystemName == "Catalog");
                if (mainMenuNode != null)
                    mainMenuNode.ChildNodes.Add(pluginNode);
                else
                    eventMessage.RootMenuItem.ChildNodes.Add(pluginNode);
            }
        }

        /// <summary>
        /// Handle the delete product attribute value event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeValue> eventMessage)
        {
            //handle event
            var pav = eventMessage.Entity;
            if (pav == null)
                return;

            //get all product attribute value condtions and delete with cross references
            var pavConditions = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(pav.Id, true);
            foreach (var pavCondition in pavConditions)
                await _productExtendService.DeleteProductAttributeValueConditionAsync(pavCondition);
        }

        /// <summary>
        /// Handle the page rendering event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            //check is admin area request
            var routeValues = _actionContextAccessor.ActionContext.RouteData.Values;
            var areaExist = routeValues.ContainsKey("area") ? (routeValues["area"]?.ToString() ?? string.Empty).Equals("Admin", StringComparison.InvariantCultureIgnoreCase) : false;
            if (areaExist)
                return;

            //ensure that product extend plugin is installed
            var descriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(ProductExtensionDefaults.SystemName, LoadPluginsMode.InstalledOnly);
            if (descriptor == null)
                return;

            //load style sheet
            eventMessage.Helper.AppendCssFileParts("~/Plugins/Widgets.ProductExtension/Content/Css/styles.css");

            //load script at footer
            eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/Plugins/Widgets.ProductExtension/Content/js/public.productextension.js");

            if (!await _settingService.GetSettingByKeyAsync("tabsettings.enableproductreviewstab", false))
                eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/Plugins/Widgets.ProductExtension/Content/js/public.productreview.js");
        }

        #endregion
    }
}
