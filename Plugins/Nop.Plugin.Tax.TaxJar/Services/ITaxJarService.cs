using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Tax.TaxJar.Domain;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// Taxjar service
/// </summary>
public partial interface ITaxJarService
{
    /// <summary>
    /// Calculated price
    /// </summary>
    /// <param name="price">Price</param>
    /// <param name="percent">Percent</param>
    /// <param name="increase">Increase</param>
    /// <returns>New price</returns>
    decimal CalculatePrice(decimal price, decimal percent, bool increase);

    /// <summary>
    /// Calculated rate
    /// </summary>
    /// <param name="price">Price</param>
    /// <param name="percent">Percent</param>
    /// <param name="increase">Increase</param>
    /// <returns>New price</returns>
    decimal CalculateRate(decimal price, decimal old);

    /// <summary>
    /// Print sales tax exemption certificate to PDF
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="customer">Customer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task PrintSalesTaxExemptionCertificateToPdfAsync(Stream stream, Customer customer);

    /// <summary>
    /// Get order items from the passed form
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="form">Form values</param>
    /// <param name="orderShippingTotalInclTax">Shipping Total Incl Tax</param>
    /// <param name="orderShippingTotalExclTax">Shipping Total Excl Tax</param>
    /// <param name="paymentAdditionalFeeInclTax">Payment Additional Fee Incl Tax</param>
    /// <param name="paymentAdditionalFeeExclTax">Payment Additional Fee Excl Tax</param>
    /// <param name="errors">Errors</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refund order. Refund order items. Tax rates (of order sub total)
    /// </returns>
    Task<(RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, SortedDictionary<decimal, decimal> taxRates)> ParseRefundOrderAsync(
        Order order, IFormCollection form, decimal orderShippingTotalInclTax, decimal orderShippingTotalExclTax,
        decimal paymentAdditionalFeeInclTax, decimal paymentAdditionalFeeExclTax, List<string> errors);

    /// <summary>
    /// Selected refunds an order (from admin panel)
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a list of errors; empty list if no errors
    /// </returns>
    Task<IList<string>> RefundSelectedAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems);

    /// <summary>
    /// Selected refunds an order (offline)
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task RefundSelectedOfflineAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems);

    /// <summary>
    /// Resend invoice to customer
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="email">Email to send invoice</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task ResendOrderInvoiceAsync(Order order, string email);
}
