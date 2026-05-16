using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Models;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.Inventory.Order.Services
{
    /// <summary>
    /// Represents the po order service
    /// </summary>
    public partial class PoOrderService : IPoOrderService
    {
        #region Fields
        protected readonly IRepository<PoOrder> _poorderRepository;
        protected readonly IRepository<PoOrderItem> _poorderitemRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly IRepository<ProductDimensions> _productdimensionsRepository;
        protected readonly IRepository<ProductManufacturer> _productManufactureRepository;
        protected readonly IRepository<OrderItem> _orderItemRepository;
        protected readonly IRepository<Core.Domain.Orders.Order> _orderRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductDimensions> _productDimensionsRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<PoOrder> _poOrderRepository;
        protected readonly IRepository<PoOrderItem> _poOrderItemRepository;
        protected readonly IRepository<OrderAssociatedProductMap> _orderAssociatedProductMapRepository;
        protected readonly InventoryOrderSettings _inventoryOrderSettings;
        #endregion

        #region Ctor
        public PoOrderService(IRepository<PoOrder> poorderRepository,
            IRepository<PoOrderItem> poorderitemRepository,
            IRepository<Product> productRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductDimensions> productdimensionsRepository,
            IRepository<ProductManufacturer> productManufactureRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Core.Domain.Orders.Order> orderRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductDimensions> productDimensionsRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<PoOrder> poOrderRepository,
            IRepository<PoOrderItem> poOrderItemRepository,
            IRepository<OrderAssociatedProductMap> orderAssociatedProductMapRepository,
            InventoryOrderSettings inventoryOrderSettings)
        {
            _poorderRepository = poorderRepository;
            _poorderitemRepository = poorderitemRepository;
            _productRepository = productRepository;
            _manufacturerRepository = manufacturerRepository;
            _productdimensionsRepository = productdimensionsRepository;
            _productManufactureRepository = productManufactureRepository;
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _productCategoryRepository = productCategoryRepository;
            _productDimensionsRepository = productDimensionsRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _poOrderRepository = poOrderRepository;
            _poOrderItemRepository = poOrderItemRepository;
            _orderAssociatedProductMapRepository = orderAssociatedProductMapRepository;
            _inventoryOrderSettings = inventoryOrderSettings;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Data
        /// </summary>
        /// <param name="poOrderItems">poOrderItems</param>
        /// <param name="id">id</param>
        /// <returns></returns>
        private string Data(IList<PoOrderItem> poOrderItems, int id)
        {
            var poOrderItem = string.Join(",", poOrderItems.Any(x => x.ProductId == id) ? poOrderItems.Where(x => x.ProductId == id).Select(x => x.OrderedQty).ToArray() : new int[] { 0, 0, 0, 0 });
            return poOrderItem;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieve a paged list of purchase orders based on filter criteria
        /// </summary>
        /// <param name="poNumber">Purchase order number</param>
        /// <param name="adminComment">Admin comment filter</param>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to include hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged list of purchase orders
        /// </returns>
        public async Task<IPagedList<PoOrder>> GetAllPoOrder(string poNumber = "", string adminComment = "",
            DateTime? startDate = null, DateTime? endDate = null, int pageIndex = 0, int pageSize = 2147483647, bool showHidden = false)
        {
            var query = _poorderRepository.Table;

            if (!string.IsNullOrWhiteSpace(poNumber))
                query = query.Where(e => e.PONumber.Contains(poNumber));

            if (!string.IsNullOrWhiteSpace(adminComment))
                query = query.Where(e => e.Comment.Contains(adminComment));

            if (startDate.HasValue)
                query = query.Where(o => startDate.Value <= o.CreatedOnUTC);

            if (endDate.HasValue)
                query = query.Where(o => endDate.Value >= o.CreatedOnUTC);

            query = query.OrderByDescending(o => o.Id);

            var poorder = new PagedList<PoOrder>(await query.ToListAsync(), pageIndex, pageSize);

            return poorder;
        }

        /// <summary>
        /// Get multiple purchase orders by identifiers
        /// </summary>
        /// <param name="poOrderIds">Array of purchase order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the list of purchase orders
        /// </returns>
        public virtual async Task<IList<PoOrder>> GetPoOrderByIdsAsync(int[] poOrderIds)
        {
            return await _poorderRepository.GetByIdsAsync(poOrderIds, includeDeleted: false);
        }

        /// <summary>
        /// Get a purchase order by identifier
        /// </summary>
        /// <param name="id">Purchase order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order
        /// </returns>
        public virtual async Task<PoOrder> GetPoOrderById(int id)
        {
            if (id == 0)
                throw new ArgumentNullException(nameof(id));

            return await _poorderRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Insert a new purchase order
        /// </summary>
        /// <param name="poOrder">Purchase order entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertPoOrder(PoOrder poOrder)
        {
            ArgumentNullException.ThrowIfNull(poOrder);

            await _poorderRepository.InsertAsync(poOrder);
        }

        /// <summary>
        /// Update an existing purchase order
        /// </summary>
        /// <param name="poOrder">Purchase order entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdatePoOrder(PoOrder poOrder)
        {
            ArgumentNullException.ThrowIfNull(poOrder);

            await _poorderRepository.UpdateAsync(poOrder);
        }

        #region PoOrderItem Method

        /// <summary>
        /// Retrieve purchase order items for a specific purchase order
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to include hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged list of purchase order items
        /// </returns>
        public virtual async Task<IPagedList<PoOrderItem>> GetPoOrderItemsByPoOrderAsync(int poOrderId = 0, int pageIndex = 0, int pageSize = 2147483647, bool showHidden = false)
        {
            var query = _poorderitemRepository.Table;

            if (poOrderId > 0)
                query = query.Where(si => si.PoOrderId == poOrderId);

            var poOrder = new PagedList<PoOrderItem>(await query.ToListAsync(), pageIndex, pageSize);

            return poOrder;
        }

        /// <summary>
        /// Get a purchase order item by identifier
        /// </summary>
        /// <param name="id">Purchase order item identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        public virtual async Task<PoOrderItem> GetByIdPoOrderItem(int id)
        {
            if (id == 0)
                throw new ArgumentNullException(nameof(id));

            return await _poorderitemRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Insert a new purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertPoOrderItem(PoOrderItem poOrderItem)
        {
            ArgumentNullException.ThrowIfNull(poOrderItem);

            await _poorderitemRepository.InsertAsync(poOrderItem);
        }

        /// <summary>
        /// Update an existing purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdatePoOrderitem(PoOrderItem poOrderItem)
        {
            ArgumentNullException.ThrowIfNull(poOrderItem);

            await _poorderitemRepository.UpdateAsync(poOrderItem);
        }

        /// <summary>
        /// Delete a purchase order item
        /// </summary>
        /// <param name="poOrderItem">Purchase order item entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeletePoOrderitem(PoOrderItem poOrderItem)
        {
            await _poorderitemRepository.DeleteAsync(poOrderItem);
        }

        /// <summary>
        /// Get a purchase order item by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        public virtual async Task<PoOrderItem> GetPoOrderItemByProductId(int productId)
        {
            var query = from poi in _poOrderItemRepository.Table
                        join po in _poOrderRepository.Table on poi.PoOrderId equals po.Id
                        where poi.ProductId == productId && po.AvailableDateOnUTC > DateTime.UtcNow && poi.OrderedQty > 0 && po.HasReceived == false
                        orderby po.AvailableDateOnUTC
                        select poi;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get a purchase order item by purchase order and product identifiers
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the purchase order item
        /// </returns>
        public virtual async Task<PoOrderItem> GetPoOrderItemByPoOrderIdAndProductId(int poOrderId, int productId)
        {
            var query = from poi in _poOrderItemRepository.Table
                        where poi.ProductId == productId && poi.OrderedQty > 0 && poi.PoOrderId == poOrderId
                        select poi;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Product Dimensions Method

        /// <summary>
        /// Insert product dimension details
        /// </summary>
        /// <param name="productDimensions">Product dimension entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductDimensions(ProductDimensions productDimensions)
        {
            ArgumentNullException.ThrowIfNull(productDimensions);

            await _productdimensionsRepository.InsertAsync(productDimensions);
        }

        /// <summary>
        /// Update product dimension details
        /// </summary>
        /// <param name="productDimensions">Product dimension entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductDimensions(ProductDimensions productDimensions)
        {
            ArgumentNullException.ThrowIfNull(productDimensions);

            await _productdimensionsRepository.UpdateAsync(productDimensions);
        }

        /// <summary>
        /// Get product dimensions by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the product dimensions
        /// </returns>
        public virtual async Task<ProductDimensions> GetProductDimensionsByProductId(int productId)
        {
            if (productId == 0)
                throw new ArgumentNullException(nameof(productId));

            var query = (from pp in _productdimensionsRepository.Table
                         where pp.ProductId == productId
                         select pp).FirstOrDefaultAsync();

            return await query;
        }

        #endregion

        #region Order Associated Product Map

        /// <summary>
        /// Insert order associated product mapping
        /// </summary>
        /// <param name="orderAssociatedProductMap">Order associated product map entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertOrderAssociatedProductMap(OrderAssociatedProductMap orderAssociatedProductMap)
        {
            ArgumentNullException.ThrowIfNull(orderAssociatedProductMap);

            await _orderAssociatedProductMapRepository.InsertAsync(orderAssociatedProductMap);
        }

        /// <summary>
        /// Delete order associated product mappings by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteOrderAssociatedProductMaps(int orderId)
        {
            var query = from oap in _orderAssociatedProductMapRepository.Table
                        where oap.OrderId == orderId
                        select oap;

            await _orderAssociatedProductMapRepository.DeleteAsync(query.ToList());
        }

        #endregion

        #region Inventory Search

        /// <summary>
        /// Prepare the manage order report model for inventory analysis
        /// </summary>
        /// <param name="startDateValue">Report start date</param>
        /// <param name="endDateValue">Report end date</param>
        /// <param name="searchManufacturerId">Manufacturer filter</param>
        /// <param name="searchCategoryId">Category filter</param>
        /// <param name="searchPercentage">Percentage filter</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation  
        /// The task result contains the paged manage order report model
        /// </returns>
        public virtual async Task<IPagedList<MangeOrderReportModel>> PrepareMangeOrderReportModel(DateTime? startDateValue, DateTime? endDateValue, int? searchManufacturerId, int? searchCategoryId, int? searchPercentage, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productAttributeItemsId = _productAttributeMappingRepository.Table.Select(x => x.ProductId).Distinct();

            //Sub queries
            var orderItems = from o in _orderRepository.Table
                             join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                             where o.CreatedOnUtc >= startDateValue && o.CreatedOnUtc <= endDateValue
                             group new { o.CreatedOnUtc, oi.Quantity }
                              by new { oi.ProductId } into g
                             select new { ProductId = g.Key.ProductId, Quantity = g.Sum(g => g.Quantity), CreatedOnUtc = g.Max(g => g.CreatedOnUtc) };

            //Sub queries
            var orderAssociatedProductMap = from oap in _orderAssociatedProductMapRepository.Table
                                            where oap.CreatedOnUtc >= startDateValue && oap.CreatedOnUtc <= endDateValue
                                            group new { oap.Quantity }
                                            by new { oap.ProductId } into g
                                            select new { ProductId = g.Key.ProductId, Quantity = g.Sum(g => g.Quantity) };

            //Sub queries
            //Only this query returns db call
            var poOrderItems = await (from po in _poOrderRepository.Table
                                      join poi in _poOrderItemRepository.Table on po.Id equals poi.PoOrderId
                                      where !po.HasReceived
                                      orderby po.CreatedOnUTC
                                      select poi).ToListAsync();

            var orderInventoryReport = from p in _productRepository.Table
                                       join pm in _productManufactureRepository.Table
                                       on p.Id equals pm.ProductId
                                       join m in _manufacturerRepository.Table
                                       on pm.ManufacturerId equals m.Id
                                       //left Join
                                       join oi in orderItems on p.Id equals oi.ProductId into ois
                                       from oi in ois.DefaultIfEmpty()
                                           //left join
                                       join pcm in _productCategoryRepository.Table
                                       on p.Id equals pcm.ProductId into pcs
                                       from pcm in pcs.DefaultIfEmpty()
                                           //left Join
                                       join oapm in orderAssociatedProductMap on p.Id equals oapm.ProductId into oapml
                                       from oapm in oapml.DefaultIfEmpty()
                                           //left Join
                                       join pd in _productDimensionsRepository.Table
                                       on p.Id equals pd.ProductId into pds
                                       from pd in pds.DefaultIfEmpty()
                                           //Get only deleted false product that having ManageInventoryMethodId = 1
                                       where !p.Deleted && p.ManageInventoryMethodId == 1 && p.ProductTypeId != (int)ProductType.GroupedProduct &&
                                       !productAttributeItemsId.Contains(p.Id) && !m.Deleted &&
                                       _inventoryOrderSettings.AllowedManufacturers.Contains(m.Id)
                                       //Group By
                                       group new { orderItem = oi.Quantity, p.StockQuantity, p.MinStockQuantity, pd.DimensionsHeight, pd.DimensionsLength, pd.DimensionsWidth, pd.QtyCartoon, pcm.CategoryId }
                                       by new { p.Id, p.Name, p.Sku, p.ProductCost, p.Published, mName = m.Name, mId = m.Id, p.StockQuantity, oi.Quantity, associatedQty = oapm.Quantity } into g
                                       orderby g.Key.mName, g.Key.Sku
                                       //Select New
                                       select new MangeOrderReportModel
                                       {
                                           ManufactureId = g.Key.mId,
                                           ProductId = g.Key.Id,
                                           Manufacture = g.Key.mName,
                                           Name = g.Key.Name,
                                           Sku = g.Key.Sku,
                                           InStock = (g.Max(g => g.StockQuantity) + g.Max(g => g.MinStockQuantity)),
                                           QTY = (g.Key.Quantity + g.Key.associatedQty),
                                           Needed = (int)Math.Ceiling(Convert.ToDecimal((g.Key.Quantity + g.Key.associatedQty)) + ((g.Key.Quantity + g.Key.associatedQty) * searchPercentage.Value / 100)),
                                           BoxVolume = g.Max(g => g.DimensionsHeight * g.DimensionsLength * g.DimensionsWidth / 1000000),
                                           TotalCartoon = g.Max(g => g.QtyCartoon) == 0 ? 1 : g.Max(g => g.QtyCartoon),
                                           OrderedQty = Data(poOrderItems, g.Key.Id),
                                           CategoryId = g.Max(g => g.CategoryId),
                                           ProductCost = g.Key.ProductCost,
                                           Published = g.Key.Published
                                       };

            //Filters
            if (searchManufacturerId.HasValue && searchManufacturerId.Value > 0)
                orderInventoryReport = orderInventoryReport.Where(x => x.ManufactureId == searchManufacturerId);

            //Filters
            if (searchCategoryId.HasValue && searchCategoryId.Value > 0)
                orderInventoryReport = orderInventoryReport.Where(x => x.CategoryId == searchCategoryId);

            //Only this query returns db call 
            return await orderInventoryReport.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Delete a purchase order by identifier
        /// </summary>
        /// <param name="poOrderId">Purchase order identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeletePoOrder(int poOrderId)
        {
            var poOrder = await GetPoOrderById(poOrderId);

            if (poOrder == null)
                return;

            var poOrderItems = await GetPoOrderItemsByPoOrderAsync(poOrderId);

            await _poorderitemRepository.DeleteAsync(poOrderItems);

            await _poOrderRepository.DeleteAsync(poOrder);
        }

        #endregion

        #endregion
    }
}
