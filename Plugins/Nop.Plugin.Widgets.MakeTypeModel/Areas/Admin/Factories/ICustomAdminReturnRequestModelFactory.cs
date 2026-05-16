using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories
{
    /// <summary>
    /// Represent custom admin retune request model factory interface
    /// </summary>
    public interface ICustomAdminReturnRequestModelFactory
    {
        /// <summary>
        /// Prepare return request model
        /// </summary>
        /// <param name="model">Return request model</param>
        /// <param name="returnRequest">Return request</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the return request model
        /// </returns>
        Task<OverrideReturnRequestModel> PrepareReturnRequestModelAsync(OverrideReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties = false);
    }
}
