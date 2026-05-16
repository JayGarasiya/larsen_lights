using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Models;

namespace Nop.Plugin.Misc.Inventory.Order.AutoMapper
{
    /// <summary>
    /// AutoMapper configuration for product dimensions model
    /// </summary>
    public class AutoMappersConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor
        public AutoMappersConfiguration()
        {
            CreateProductDimessionMaps();
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Create product dimensions maps 
        /// </summary>
        protected virtual void CreateProductDimessionMaps()
        {
            CreateMap<ProductDimensions, ProductDimensionsModel>();
            CreateMap<ProductDimensionsModel, ProductDimensions>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;
        #endregion
    }
}
