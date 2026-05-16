using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.CustomGenericAttribute
{
    /// <summary>
    /// Custom generic attributes service
    /// </summary>
    public class CustomGenericAttributeService : ICustomGenericAttributeService
    {
        #region Fields

        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion
         
        #region Ctor

        public CustomGenericAttributeService(IRepository<GenericAttribute> genericAttributeRepository,
            IStaticCacheManager staticCacheManager)
        {
            _genericAttributeRepository = genericAttributeRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="keyGroup">Key group</param>
        /// <returns>
        /// A task that represents the asynchronous operation 
        /// The task result contains the get attributes
        /// </returns>
        public virtual async Task<IList<GenericAttribute>> GetAttributesForEntitiesAsync(int[] entityId, string keyGroup)
        {
            if (entityId == null || entityId.Length == 0)
                return new List<GenericAttribute>();

            var key = _staticCacheManager.PrepareKey(NopCommonDefaults.GenericAttributeCacheKey, entityId, keyGroup);

            var query = from ga in _genericAttributeRepository.Table
                        where entityId.Contains(ga.EntityId) &&
                              ga.KeyGroup == keyGroup
                        select ga;

            var attributes = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

            return attributes;
        }

        #endregion
    }
}
