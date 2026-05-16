using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.MakeTypeModel.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents a custom return request model factory
    /// </summary>
    public interface ICustomReturnRequestModelFactory
    {
        /// <summary>
        /// Prepare submit return request model
        /// </summary>
        /// <param name="model">CustomSubmitReturnRequestModel</param>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<CustomSubmitReturnRequestModel> PrepareSubmitReturnRequestModelAsync(CustomSubmitReturnRequestModel model,
            Order order);
    }
}