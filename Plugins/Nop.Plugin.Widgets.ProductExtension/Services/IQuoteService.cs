using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Quote service interface
    /// </summary>
    public partial interface IQuoteService
    {
        /// <summary>
        /// Print an cart to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cart">Shopping Cart</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a path of generated file 
        /// </returns>
        Task PrintCartToPdfAsync(Stream stream, IList<ShoppingCartItem> shoppingCartItems);
    }
}