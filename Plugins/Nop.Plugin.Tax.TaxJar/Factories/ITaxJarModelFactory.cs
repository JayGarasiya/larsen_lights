using Nop.Core.Domain.Orders;
using Nop.Plugin.Tax.TaxJar.Models;

namespace Nop.Plugin.Tax.TaxJar.Factories;

/// <summary>
/// Interface for creating and preparing TaxJar-related models for orders and refunds.
/// </summary>
public partial interface ITaxJarModelFactory
{
    /// <summary>
    /// Prepares a refund order model asynchronously.
    /// </summary>
    /// <param name="model">Refund order model</param>
    /// <param name="order">The order object</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refund order model
    /// </returns>
    Task<RefundOrderModel> PrepareRefundOrderModelAsync(RefundOrderModel model, Order order);
}
