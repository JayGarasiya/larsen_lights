using Nop.Core.Domain.Common;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.CustomGenericAttribute
{
    /// <summary>
    /// Custom generic attributes service
    /// </summary>
    public interface ICustomGenericAttributeService
    {
        /// <summary>
        /// Get attributes for entity
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="keyGroup">Key group</param>
        /// <returns>
        /// A task that represents the asynchronous operation 
        /// The task result contains the get attributes
        /// </returns>
        Task<IList<GenericAttribute>> GetAttributesForEntitiesAsync(int[] entityId, string keyGroup);
    }
}
 