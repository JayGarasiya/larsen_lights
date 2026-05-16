using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.ShipmentTracking.Domain;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.ShipmentTracking.Services
{
    /// <summary>
    /// Represents an event consumer
    /// </summary>
    public class EventConsumer : IConsumer<EntityTokensAddedEvent<Shipment>>
    {
        #region Constant 

        private const string ShippingMethod = "3PLCarrier";

        #endregion

        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;       

        #endregion

        #region Ctor

        public EventConsumer(IGenericAttributeService genericAttributeService)
        {
            _genericAttributeService = genericAttributeService;           
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle the token add event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityTokensAddedEvent<Shipment> eventMessage)
        {
            var shipment = eventMessage.Entity;
            var tokens = eventMessage.Tokens;
            var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
            if (!string.IsNullOrEmpty(shippingMethod) && !string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                //Add custom token shipping method
                tokens.Add(new Token("Shipment.ShipmentMethod", shippingMethod));

                //Find tracking url token to replace with new url
                var trackingUrlToken = tokens.FirstOrDefault(t => t.Key.Equals("Shipment.TrackingNumberURL"));
                if (shippingMethod.Equals(ShipmentMethod.UPS.ToString()))
                {
                    tokens.Remove(trackingUrlToken);
                    tokens.Add(new Token("Shipment.TrackingNumberURL", $"https://www.ups.com/track?&tracknum={shipment.TrackingNumber}", true));
                }
                else if (shippingMethod.Equals(ShipmentMethod.USPS.ToString()))
                {
                    tokens.Remove(trackingUrlToken);
                    tokens.Add(new Token("Shipment.TrackingNumberURL", $"https://tools.usps.com/go/TrackConfirmAction?tLabels={shipment.TrackingNumber}", true));
                }
                else if (shippingMethod.Equals(ShipmentMethod.FEDEX.ToString()))
                {
                    tokens.Remove(trackingUrlToken);
                    tokens.Add(new Token("Shipment.TrackingNumberURL", $"https://www.fedex.com/apps/fedextrack/?action=track&tracknumbers={shipment.TrackingNumber}", true));
                }
                else if (shippingMethod.Equals(ShipmentMethod.SPEEDEE.ToString()))
                {
                    tokens.Remove(trackingUrlToken);
                    tokens.Add(new Token("Shipment.TrackingNumberURL", $"https://speedeedelivery.com/track-a-shipment/?barcodes={shipment.TrackingNumber}", true));
                }
                else if (shippingMethod.Equals(ShipmentMethod.DHL.ToString()))
                {
                    tokens.Remove(trackingUrlToken);
                    tokens.Add(new Token("Shipment.TrackingNumberURL", $"https://www.dhl.com/global-en/home/tracking/tracking-express.html?submit=1&tracking-id={shipment.TrackingNumber}", true));
                }
            }
        }

        #endregion
    }
}
