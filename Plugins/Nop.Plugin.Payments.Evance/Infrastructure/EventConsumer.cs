using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Plugin.Payments.Evance.Infrastructure
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public EventConsumer(IGenericAttributeService genericAttributeService, ICustomerService customerService, IHttpContextAccessor httpContextAccessor)
        {
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var orderGuid = eventMessage.Order.OrderGuid.ToString();
            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Order.CustomerId);
            var genericAttribute = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, customer.GetType().Name))
                .Where(x => x.Key == eventMessage.Order.OrderGuid.ToString())
                .FirstOrDefault();

            var genericAttributeRequest = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, customer.GetType().Name))
               .Where(x => x.Key == $"{orderGuid}- request")
               .FirstOrDefault();

            if(genericAttribute != null)
                await _genericAttributeService.DeleteAttributeAsync(genericAttribute);

            if (genericAttributeRequest != null)
                await _genericAttributeService.DeleteAttributeAsync(genericAttributeRequest);

            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        #endregion
    }
}
