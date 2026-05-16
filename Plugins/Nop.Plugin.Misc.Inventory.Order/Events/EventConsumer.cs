using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.Inventory.Order.Events
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public partial class EventConsumer : IConsumer<OrderStatusChangedEvent>, IConsumer<OrderPlacedEvent>, IConsumer<AdminMenuCreatedEvent>
    {
        #region Fields
        protected readonly IOrderService _orderService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductService _productService;
        protected readonly IPoOrderService _poOrderService;
        protected readonly WidgetSettings _widgetSettings;
        protected readonly ICustomerService _customerService;
        protected readonly IWorkContext _workContext;
        protected readonly ILocalizationService _localizationService;
        protected readonly IAdminMenu _adminMenu;
        #endregion

        #region Ctor
        public EventConsumer(IOrderService orderService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IPoOrderService poOrderService,
            WidgetSettings widgetSettings,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IAdminMenu adminMenu)
        {
            _orderService = orderService;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _poOrderService = poOrderService;
            _widgetSettings = widgetSettings;
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
            _adminMenu = adminMenu;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            var order = eventMessage.Order;

            if (order.OrderStatus == OrderStatus.Cancelled)
                await _poOrderService.DeleteOrderAssociatedProductMaps(order.Id);
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            //validate if any order item with attribute
            orderItems = orderItems.Where(oi => !string.IsNullOrEmpty(oi.AttributesXml)).ToList();
            foreach (var oitem in orderItems)
            {
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(oitem.AttributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.AttributeValueType != AttributeValueType.AssociatedToProduct)
                        continue;

                    //associated product (bundle)
                    var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                    if (associatedProduct == null)
                        continue;

                    var orderAssociatedProductMap = new OrderAssociatedProductMap()
                    {
                        OrderId = order.Id,
                        ProductId = associatedProduct.Id,
                        Quantity = oitem.Quantity * attributeValue.Quantity,
                        CreatedOnUtc = order.CreatedOnUtc,
                        OrderItemId = oitem.Id
                    };

                    await _poOrderService.InsertOrderAssociatedProductMap(orderAssociatedProductMap);
                }
            }
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(InventoryOrderDefault.SystemName))
            {
                if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                    return;

                var mainMenu = new AdminMenuItem()
                {
                    SystemName = "nopCommercePlus",
                    Title = "nopCommercePlus",
                    IconClass = "fas fa-signature",
                    Visible = true
                };

                var pluginMenu = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Misc.Inventory.Order.PoOrderTitle",
                    Title = await _localizationService.GetResourceAsync("Plugins.InventoryOrder.Fields.PoOrderTitle"),
                    IconClass = "far fa-dot-circle",
                    Visible = true
                };

                var configurationMenuItem = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Misc.Inventory.Order.Configuration",
                    Title = await _localizationService.GetResourceAsync("Plugins.InventoryOrder.Fields.Configuration"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = _adminMenu.GetMenuItemUrl("InventoryOrder", "Configure")
                };

                var createpoorderMenuItem = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Misc.Inventory.Order.CreatePoOrder",
                    Title = await _localizationService.GetResourceAsync("Plugins.InventoryOrder.Fields.CreatePoOrderTitle"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = _adminMenu.GetMenuItemUrl("InventoryOrder", "List")
                };

                var managepoorderitemsMenuItem = new AdminMenuItem()
                {
                    SystemName = "Nop.Plugin.Misc.Inventory.Order.ManagePoOrderItems",
                    Title = await _localizationService.GetResourceAsync("Plugins.InventoryOrder.Fields.ManagePoOrderItemsTitle"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = _adminMenu.GetMenuItemUrl("InventoryOrder", "ManagePoOrder")
                };

                pluginMenu.ChildNodes.Add(configurationMenuItem);
                pluginMenu.ChildNodes.Add(createpoorderMenuItem);
                pluginMenu.ChildNodes.Add(managepoorderitemsMenuItem);

                var nopMenuRoot = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName.Equals("nopCommercePlus"));
                if (nopMenuRoot == null)
                {
                    mainMenu.ChildNodes.Add(pluginMenu);
                    eventMessage.RootMenuItem.ChildNodes.Add(mainMenu);
                }
                else
                    nopMenuRoot.ChildNodes.Add(pluginMenu);
            }
        }
        #endregion
    }
}
