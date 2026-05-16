using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.MakeTypeModel.Events
{
    /// <summary>
    /// Represents a pluign event consumer
    /// </summary>
    public class EventConsumer : IConsumer<EntityTokensAddedEvent<ReturnRequest>>, IConsumer<AdminMenuCreatedEvent>, IConsumer<OrderPlacedEvent>, IConsumer<AdditionalTokensAddedEvent>

    {
        #region Fields

        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IOrderService _orderService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductService _productService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IAdminMenu _adminMenu;
        protected readonly WidgetSettings _widgetSettings;
        protected readonly IPermissionService _permissionService;
        private readonly MakeTypeModelSettings _makeTypeModelSettings;

        #endregion

        #region Ctor 

        public EventConsumer(IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ILocalizationService localizationService,
            IAdminMenu adminMenu,
            WidgetSettings widgetSettings,
            IPermissionService permissionService,
            MakeTypeModelSettings makeTypeModelSettings)
        {
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _localizationService = localizationService;
            _adminMenu = adminMenu;
            _widgetSettings = widgetSettings;
            _permissionService = permissionService;
            _makeTypeModelSettings = makeTypeModelSettings;
        }

        #endregion

        #region Method

        /// <summary>
        /// Handle token added event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityTokensAddedEvent<ReturnRequest> eventMessage)
        {
            var genericAttributes = await _genericAttributeService.GetAttributesForEntityAsync(eventMessage.Entity.Id, "ReturnRequest");
            var genericAttribute = genericAttributes.FirstOrDefault();

            if (genericAttribute == null || !int.TryParse(genericAttribute.Value, out var attributeId))
                return;

            var orderItem = await _orderService.GetOrderItemByIdAsync(eventMessage.Entity.OrderItemId);
            if (orderItem == null)
                return;

            var parsedAttributes =( await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml)).Where(x=>x.Id == attributeId).FirstOrDefault();
            var associatedProduct = await _productService.GetProductByIdAsync(parsedAttributes.AssociatedProductId);

            if (associatedProduct != null)
            {
                var existingToken = eventMessage.Tokens.FirstOrDefault(t => t.Key == "ReturnRequest.Product.Name");
                if (existingToken != null)
                {
                    eventMessage.Tokens.Remove(existingToken);
                }

                eventMessage.Tokens.Add(new Token("ReturnRequest.Product.Name", associatedProduct.Name));
            }

            return;
        }

        /// <summary>
        /// Handle admin menu created event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            {
                var navigationTitle = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.Title");
                var siteMapNode = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.SystemName,
                    Title = navigationTitle,
                    IconClass = "fas fa-puzzle-piece",
                    Visible = true
                };
                var makeProduct = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.MakeProduct,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.MakeProduct").Result,
                    Url = "/Admin/MakeTypeModel/MakeList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var typeProduct = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.TypeProduct,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.TypeProduct").Result,
                    Url = "/Admin/MakeTypeModel/TypeList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var modelProduct = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.ModelProduct,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.ModelProduct").Result,
                    Url = "/Admin/MakeTypeModel/ModelList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var category = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.ModelCategory,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.ModelCategory").Result,
                    Url = "/Admin/MakeTypeModel/CategoryList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var productImport = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.ProductImports,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.ProductImports").Result,
                    Url = "/Admin/MakeTypeModel/ProductImportList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var granitImport = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.GranitProductImports,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.GranitProductImports").Result,
                    Url = "/Admin/MakeTypeModel/GranitProductImportList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var priceImport = new AdminMenuItem()
                {
                    SystemName = MakeTypeModelDefaults.PriceImports,
                    Title = _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.Navigation.PriceImports").Result,
                    Url = "/Admin/MakeTypeModel/PriceImportsList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                siteMapNode.ChildNodes.Add(makeProduct);
                siteMapNode.ChildNodes.Add(typeProduct);
                siteMapNode.ChildNodes.Add(modelProduct);
                siteMapNode.ChildNodes.Add(category);
                siteMapNode.ChildNodes.Add(productImport);
                siteMapNode.ChildNodes.Add(granitImport);
                siteMapNode.ChildNodes.Add(priceImport);

                var mainMenuNode = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName == MakeTypeModelDefaults.SystemName);
                if (mainMenuNode != null)
                {
                    mainMenuNode.ChildNodes.Add(makeProduct);
                    mainMenuNode.ChildNodes.Add(typeProduct);
                    mainMenuNode.ChildNodes.Add(modelProduct);
                    mainMenuNode.ChildNodes.Add(category);
                    mainMenuNode.ChildNodes.Add(productImport);
                    mainMenuNode.ChildNodes.Add(granitImport);
                    mainMenuNode.ChildNodes.Add(priceImport);
                }
                else
                {
                    var catalogMap = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName.Equals("Catalog"));
                    var catalogIndex = eventMessage.RootMenuItem.ChildNodes.IndexOf(catalogMap);
                    eventMessage.RootMenuItem.ChildNodes.Insert(catalogIndex + 1, siteMapNode);
                }
            }
        }

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (!_makeTypeModelSettings.EnabledFreeSkus)
                return;

            var order = eventMessage.Order;
            var orderSubTotal = order.OrderSubtotalExclTax;
            var freeItemSkus = _makeTypeModelSettings.FreeItemsSkus;
            var freeItemWarehouseIds = _makeTypeModelSettings.FreeItemsWarehouseIds?.ToList() ?? new List<int>();

            if (orderSubTotal < _makeTypeModelSettings.SubTotalGreaterThan ||
                string.IsNullOrEmpty(freeItemSkus) ||
                !freeItemWarehouseIds.Any())
                return;

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            var hasEligibleWarehouseProduct = false;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product == null || product.Deleted || !product.Published)
                    continue;

                if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.DontManageStock)
                    continue;

                if (product.UseMultipleWarehouses)
                {
                    var inventories = await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id);

                    if (inventories.Any(pwi => freeItemWarehouseIds.Contains(pwi.WarehouseId)))
                    {
                        hasEligibleWarehouseProduct = true;
                        break;
                    }
                }
                else
                {
                    if (product.WarehouseId > 0 &&
                        freeItemWarehouseIds.Contains(product.WarehouseId))
                    {
                        hasEligibleWarehouseProduct = true;
                        break;
                    }
                }
            }

            if (!hasEligibleWarehouseProduct)
                return;

            var skuList = freeItemSkus
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            foreach (var sku in skuList)
            {
                var product = await _productService.GetProductBySkuAsync(sku);
                if (product == null || !product.Published || product.Deleted)
                    continue;

                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPriceExclTax = decimal.Zero,
                    UnitPriceInclTax = decimal.Zero,
                    PriceExclTax = decimal.Zero,
                    PriceInclTax = decimal.Zero,
                    ItemWeight = product.Weight
                };

                await _orderService.InsertOrderItemAsync(orderItem);
            }
        }

        /// <summary>
        /// Add custom allowed tokens 
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(AdditionalTokensAddedEvent eventMessage)
        {
            eventMessage.AdditionalTokens.Add(MakeTypeModelDefaults.ReturnRequestNoteToken);
            await Task.CompletedTask;
        }

        #endregion
    }
}
