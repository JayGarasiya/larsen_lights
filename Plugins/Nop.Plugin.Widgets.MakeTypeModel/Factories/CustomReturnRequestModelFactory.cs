using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Widgets.MakeTypeModel.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Services.CustomGenericAttribute;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Seo;
using static Nop.Plugin.Widgets.MakeTypeModel.Models.CustomSubmitReturnRequestModel;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents a custom retune request model factory
    /// </summary>
    public partial class CustomReturnRequestModelFactory : ICustomReturnRequestModelFactory
    {
        #region Fields

        protected readonly OrderSettings _orderSettings;
        protected readonly IReturnRequestService _returnRequestService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductService _productService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IWorkContext _workContext;
        protected readonly ICurrencyService _currencyService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly ICustomGenericAttributeService _customGenericAttributeService;

        #endregion

        #region Ctor

        public CustomReturnRequestModelFactory(OrderSettings orderSettings,
            IReturnRequestService returnRequestService,
            ILocalizationService localizationService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ICustomGenericAttributeService customGenericAttributeService)
        {
            _orderSettings = orderSettings;
            _returnRequestService = returnRequestService;
            _localizationService = localizationService;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _customGenericAttributeService = customGenericAttributeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepares the order item models for return request by specified order.
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// The <see cref="Task"/> containing the <see cref="IList{OrderItemModel}"/>
        /// </returns>
        protected virtual async Task<IList<OrderItemModel>> PrepareSubmitReturnRequestOrderItemModelsAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            var models = new List<OrderItemModel>();

            var returnRequestAvailability = await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id);
            if (returnRequestAvailability?.IsAllowed == true || returnRequestAvailability.ReturnableOrderItems != null)
            {
                foreach (var returnableOrderItem in returnRequestAvailability.ReturnableOrderItems)
                {
                    var orderItem = returnableOrderItem.OrderItem;
                    if (orderItem == null)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    if (product == null)
                        continue;

                    var returnRequest = (await _returnRequestService.SearchReturnRequestsAsync(orderItemId: orderItem.Id)).ToList();

                    var associatedProducts = new List<AttributeModel>();

                    if (returnRequest.Count > 0)
                    {
                        var genericAttribute = (await _customGenericAttributeService.GetAttributesForEntitiesAsync(returnRequest.Select(x=>x.Id).ToArray(), "ReturnRequest")).ToList();
                        
                        var unmatchedRequests = returnRequest.Where(x => !genericAttribute.Select(n => n.EntityId).Contains(x.Id)).FirstOrDefault();

                        var model = new OrderItemModel();
                        if (genericAttribute.Count > 0)
                        {
                            var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);

                            if (attributes != null)
                            {
                                foreach (var mappingId in attributes)
                                {
                                    if (mappingId == null)
                                        continue;

                                    var associatedProduct = await _productService.GetProductByIdAsync(mappingId.AssociatedProductId);
                                    if (associatedProduct == null || !associatedProduct.Published)
                                        continue;

                                    var quantity = mappingId.Quantity - returnRequest.Where(req => genericAttribute.Any(attr => Convert.ToInt32(attr.Value) == mappingId.Id && attr.EntityId == req.Id))
                                    .Sum(req => req.Quantity);

                                    associatedProducts.Add(new AttributeModel
                                    {
                                        Id = mappingId.Id,
                                        Name = associatedProduct.Name ?? string.Empty,
                                        Quantity = quantity,
                                        Price = associatedProduct.Price
                                    });
                                }

                            }

                            model = new OrderItemModel
                            {
                                Id = orderItem.Id,
                                ProductId = product.Id,
                                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                                Quantity = orderItem.Quantity,
                                products = associatedProducts
                            };
                        }
                        if(unmatchedRequests != null)
                        {
                            model = new OrderItemModel
                            {
                                Id = orderItem.Id,
                                ProductId = product.Id,
                                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                                Quantity = orderItem.Quantity - unmatchedRequests.Quantity,
                                products = associatedProducts
                            };
                        }
                        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                        //unit price
                        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                        {
                            //including tax
                            var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                            model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                        }
                        else
                        {
                            //excluding tax
                            var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                            model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                        }

                        models.Add(model);
                    }
                    else
                    {
                        if (returnableOrderItem.AvailableQuantityForReturn == 0)
                            continue;

                        var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);

                        if (attributes != null)
                        {
                            foreach (var mappingId in attributes)
                            {
                                if (mappingId == null)
                                    continue;

                                var associatedProduct = await _productService.GetProductByIdAsync(mappingId.AssociatedProductId);
                                if (associatedProduct == null)
                                    continue;

                                associatedProducts.Add(new AttributeModel
                                {
                                    Id = mappingId.Id,
                                    Name = associatedProduct.Name ?? string.Empty,
                                    Quantity = mappingId.Quantity,
                                    Price = associatedProduct.Price
                                });
                            }
                        }

                        var model = new OrderItemModel
                        {
                            Id = orderItem.Id,
                            ProductId = product.Id,
                            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                            Quantity = orderItem.Quantity,
                            products = associatedProducts
                        };

                        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                        //unit price
                        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                        {
                            //including tax
                            var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                            model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                        }
                        else
                        {
                            //excluding tax
                            var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                            model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                        }

                        models.Add(model);
                    }
                }
            }

            return models;
        }

        #endregion

        #region Method

        /// <summary>
        /// Prepare the submit return request model
        /// </summary>
        /// <param name="model">Submit return request model</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the submit return request model
        /// </returns>
        public virtual async Task<CustomSubmitReturnRequestModel> PrepareSubmitReturnRequestModelAsync(CustomSubmitReturnRequestModel model,
            Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            ArgumentNullException.ThrowIfNull(model);

            model.OrderId = order.Id;
            model.AllowFiles = _orderSettings.ReturnRequestsAllowFiles;
            model.CustomOrderNumber = order.CustomOrderNumber;

            //return reasons
            model.AvailableReturnReasons = await (await _returnRequestService.GetAllReturnRequestReasonsAsync())
                .SelectAwait(async rrr => new ReturnRequestReasonModel
                {
                    Id = rrr.Id,
                    Name = await _localizationService.GetLocalizedAsync(rrr, x => x.Name)
                }).ToListAsync();

            //return actions
            model.AvailableReturnActions = await (await _returnRequestService.GetAllReturnRequestActionsAsync())
                .SelectAwait(async rra => new ReturnRequestActionModel
                {
                    Id = rra.Id,
                    Name = await _localizationService.GetLocalizedAsync(rra, x => x.Name)
                })
                .ToListAsync();

            //returnable products
            model.Items = await PrepareSubmitReturnRequestOrderItemModelsAsync(order);

            return model;
        }

        #endregion
    }
}
