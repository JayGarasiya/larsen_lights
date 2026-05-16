using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Common
{
    /// <summary>
    /// Represents a 3PL central default
    /// </summary>
    internal static class ThreePlDefault
    {
        /// <summary>
        /// Add or update records
        /// </summary>
        /// <param name="items">items</param>
        /// <param name="item">item</param>
        public static void AddOrUpdateRecords(this List<OrdersItemDto> items, OrdersItemDto item)
        {
            var existRecord = items.Where(s => s.ExternalId.Equals(item.ExternalId)).FirstOrDefault();
            if (existRecord != null)
            {
                items.ForEach(i =>
                {
                    if (i.ExternalId.Equals(item.ExternalId))
                    {
                        existRecord.Qty += item.Qty;
                        existRecord.FulfillInvSalePrice += item.FulfillInvSalePrice;
                    }
                });
            }
            else
                items.Add(item);
        }

        /// <summary>
        /// Remove or update records
        /// </summary>
        /// <param name="items">items</param>
        /// <param name="item">item</param>
        /// <returns></returns>
        public static bool RemoveOrUpdateRecords(this List<OrdersItemDto> items, OrdersItemDto item)
        {
            var removed = false;
            var existRecord = items.Where(s => s.ExternalId.Equals(item.ExternalId)).FirstOrDefault();
            if (existRecord != null)
            {
                items.ForEach(i =>
                {
                    if (i.ExternalId.Equals(item.ExternalId) && i.Qty >= item.Qty)
                    {
                        existRecord.Qty -= item.Qty;
                        existRecord.FulfillInvSalePrice -= item.FulfillInvSalePrice;
                        removed = true;
                    }
                });
            }

            return removed;
        }

        /// <summary>
        /// Filter already synced items
        /// </summary>
        /// <param name="nopItem">nopItem</param>
        /// <param name="orderItem3pl">orderItem3pl</param>
        /// <returns></returns>
        public static SyncOrderItem FilterAlreadySyncedItems(this SyncOrderItem nopItem, OrderItemsResultDto orderItem3pl)
        {
            if (orderItem3pl != null)
            {
                foreach (var item in orderItem3pl.Items.Items)
                {
                    //updated order items list
                    var updatedItems = new SyncOrderItem();
                    var filtered = false;
                    nopItem.InStockItems.ForEach(stock =>
                    {
                        if (!filtered && stock.ItemIdentifier.Sku.Equals(item.ItemIdentifier.Sku) && stock.Qty >= item.Qty)
                        {
                            stock.Qty -= item.Qty;
                            filtered = true;
                        }
                    });

                    nopItem.OutOfStockItems.ForEach(outstock =>
                    {
                        if (!filtered && outstock.ItemIdentifier.Sku.Equals(item.ItemIdentifier.Sku) && outstock.Qty >= item.Qty)
                        {
                            outstock.Qty -= item.Qty;
                            filtered = true;
                        }
                    });

                    var inStock = nopItem.InStockItems.Where(stock => stock.Qty > 0).ToList();
                    var outOfStock = nopItem.OutOfStockItems.Where(outstock => outstock.Qty > 0).ToList();
                    updatedItems = new SyncOrderItem()
                    {
                        OrderItemId = nopItem.OrderItemId,
                        Note = nopItem.Note,
                        InStockItems = inStock,
                        OutOfStockItems = outOfStock
                    };

                    nopItem = updatedItems;
                }
            }

            return nopItem;
        }
    }
}
