using AutoMapper;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure.Mapper;

namespace Nop.Plugin.Misc.ShipmentTracking.Infrastructure
{
    /// <summary>
    /// Represents an auto mapper configuration for admin area models
    /// </summary>
    public class AdminMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public AdminMapperConfiguration()
        {
            CreateMap<Shipment, Models.OrderShipment.ShipmentModel>()
                .ForMember(model => model.ShippedDate, options => options.Ignore())
                .ForMember(model => model.DeliveryDate, options => options.Ignore())
                .ForMember(model => model.TotalWeight, options => options.Ignore())
                .ForMember(model => model.TrackingNumberUrl, options => options.Ignore())
                .ForMember(model => model.Items, options => options.Ignore())
                .ForMember(model => model.ShipmentStatusEvents, options => options.Ignore())
                .ForMember(model => model.CanShip, options => options.Ignore())
                .ForMember(model => model.CanDeliver, options => options.Ignore())
                .ForMember(model => model.CustomOrderNumber, options => options.Ignore());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets order of this startup task implementation
        /// </summary>
        public int Order => 1;

        #endregion

    }
}
