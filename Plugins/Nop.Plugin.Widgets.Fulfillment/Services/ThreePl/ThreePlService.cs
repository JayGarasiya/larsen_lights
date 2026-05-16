using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Services.Cms;

namespace Nop.Plugin.Widgets.Fulfillment.Services.ThreePl
{
    /// <summary>
    /// Represents service 3PL central service
    /// </summary>
    public partial class ThreePlService : IThreePlService
    {
        #region Fields

        protected readonly IRepository<ThreePlRecord> _threePlRepository;
        protected readonly IRepository<ThreePlOrder> _threePlOrderRepository;
        protected readonly IRepository<ThreePlShippingMethod> _threePlShippingRepository;
        protected readonly IRepository<Order> _orderRepository;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IWidgetPluginManager _widgetPluginManager;

        #endregion

        #region Ctor

        public ThreePlService(IRepository<ThreePlRecord> threePlRepository,
            IRepository<ThreePlOrder> threePlOrderRepository,
            IRepository<ThreePlShippingMethod> threePlShippingRepository,
            IRepository<Order> orderRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWidgetPluginManager widgetPluginManager) 
        {
            _threePlRepository = threePlRepository;
            _threePlOrderRepository = threePlOrderRepository;
            _threePlShippingRepository = threePlShippingRepository;
            _orderRepository = orderRepository;
            _workContext = workContext;
            _storeContext = storeContext;
            _widgetPluginManager = widgetPluginManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether the plugin is active for the current user and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<bool> PluginActiveAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            return await _widgetPluginManager.IsPluginActiveAsync(FulfillmentDefaults.SystemName, customer, store.Id);
        }

        #region 3PL Record

        /// <summary>
        /// Get a 3PL central record by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central record
        /// </returns>
        public async Task<ThreePlRecord> GetThreePlRecordByOrderIdAsync(int orderId)
        {
            return await _threePlRepository.Table
                .FirstOrDefaultAsync(tsm => tsm.OrderId.Equals(orderId));
        }

        /// <summary>
        /// Get a 3PL central record by identifier
        /// </summary>
        /// <param name="threePlRecordId">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central record
        /// </returns>
        public async Task<ThreePlRecord> GetThreePlRecordByIdAsync(int threePlRecordId)
        {
            return await _threePlRepository.GetByIdAsync(threePlRecordId);
        }

        /// <summary>
        /// Insert the 3PL central record
        /// </summary>
        /// <param name="threePlRecord">3PL central record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertThreePlRecordAsync(ThreePlRecord threePlRecord)
        {
            await _threePlRepository.InsertAsync(threePlRecord, false);
        }

        /// <summary>
        /// Search 3PL records
        /// </summary>
        /// <param name="orderId">Order identifier; 0 to load all 3PL records</param>
        /// <param name="threePlOrderId">3PL order identifier; null to load all 3PL records</param>
        /// <param name="tsIds">3PL status identifiers; null to load all 3PL records</param>
        /// <param name="etsIds">Exclude 3PL status identifiers; null to load all 3PL records</param>
        /// <param name="onlyCheckMoneyOrder">Only paid by check money order. 0 to load all 3PL records</param>
        /// <param name="onlyCancelledOrder">Only cancelled  order. 0 to load all 3PL records</param>
        /// <param name="excludeCancelledOrder">Exclude cancelled  order. 0 to load all 3PL records</param>
        /// <param name="excludeDeletedOrder">Exclude deleted order; 0 to load all 3PL records</param>
        /// <param name="cancelledDate">Cancelled date to (UTC); null to load all 3PL records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL records
        /// </returns>
        public async Task<IPagedList<ThreePlRecord>> SearchThreePlRecordsAsync(int orderId = 0, int threePlOrderId = 0, List<int> tsIds = null, List<int> etsIds = null,
            bool onlyCheckMoneyOrder = false, bool onlyCancelledOrder = false, bool excludeCancelledOrder = false, bool excludeDeletedOrder = false,
            DateTime? cancelledDate = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _threePlRepository.Table;

            //filter by order
            if (orderId > 0)
                query = query.Where(o => o.OrderId == orderId);

            //filter by threePl order
            if (threePlOrderId > 0)
            {
                query = from o in query
                        join oi in _threePlOrderRepository.Table on o.Id equals oi.ThreePlId
                        where oi.ThreePlOrderId == threePlOrderId
                        select o;

                query = query.Distinct();
            }

            //filter by 3PL status
            if (tsIds != null && tsIds.Any())
                query = query.Where(o => tsIds.Contains(o.ThreePlStutusId));

            //filter by only Check / Money order
            if (onlyCheckMoneyOrder)
                query = query.Where(o => o.PaidByCheck);

            //filter by only canceled order
            if (onlyCancelledOrder && cancelledDate.HasValue)
                query = query.Where(o => o.CancelledDate < cancelledDate.Value);

            //filter by only exclude canceled order
            if (excludeCancelledOrder)
                query = query.Where(o => o.CancelledDate == null);

            //filter by only exclude already synced order
            if (etsIds != null && etsIds.Any())
                query = query.Where(o => !etsIds.Contains(o.ThreePlStutusId));

            if (excludeDeletedOrder)
            {
                query = from o in query
                        join oi in _orderRepository.Table on o.OrderId equals oi.Id
                        where oi.Id == o.OrderId && !oi.Deleted
                        select o;

                query = query.Distinct();
            }

            query = query.OrderByDescending(o => o.OrderId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Update the 3PL central record
        /// </summary>
        /// <param name="threePlRecord">3PL central record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateThreePlRecordAsync(ThreePlRecord threePlRecord)
        {
            await _threePlRepository.UpdateAsync(threePlRecord, false);
        }

        #endregion

        #region 3PL Order

        /// <summary>
        /// Get a 3PL central Orders by order identifier
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central Order 
        /// </returns>
        public async Task<IList<ThreePlOrder>> GetThreePlOrdersByOrderIdAsync(int orderId)
        {
            if (orderId == 0)
                return new List<ThreePlOrder>();

            var query = from threePlOrder in _threePlOrderRepository.Table
                        join threePl in _threePlRepository.Table on threePlOrder.ThreePlId equals threePl.Id
                        where orderId == threePl.OrderId
                        orderby threePlOrder.CreationDate descending, threePlOrder.Id
                        select threePlOrder;

            var threePlOrders = await query.ToListAsync();
            return threePlOrders;
        }

        /// <summary>
        /// Get a 3PL central Order by identifier
        /// </summary>
        /// <param name="threePlOrderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL central Order
        /// </returns>
        public async Task<ThreePlOrder> GetThreePlOrderByIdAsync(int threePlOrderId)
        {
            return await _threePlOrderRepository.GetByIdAsync(threePlOrderId);
        }

        /// <summary>
        /// Insert the 3PL central Order
        /// </summary>
        /// <param name="threePlOrder">3PL central Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertThreePlOrderAsync(ThreePlOrder threePlOrder)
        {
            await _threePlOrderRepository.InsertAsync(threePlOrder, false);
        }

        /// <summary>
        /// Search 3PL orders
        /// </summary>
        /// <param name="threePlId">3Pl record identifier; 0 to load all 3PL orders</param>
        /// <param name="orderId">Order identifier; 0 to load all 3PL orders</param>
        /// <param name="threePlOrderId">3PL order identifier; null to load all 3PL orders</param>
        /// <param name="tsIds">3PL status identifiers; null to load all 3PL orders</param>
        /// <param name="onlyCancelledOrder">Only cancelled  order. 0 to load all 3PL orders</param>
        /// <param name="excludeCancelledOrder">Exclude cancelled  order. 0 to load all 3PL orders</param>
        /// <param name="excludeDeletedOrder">Exclude deleted order; 0 to load all 3PL orders</param>
        /// <param name="cancelledDate">Cancelled date to (UTC); null to load all 3PL orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL orders
        /// </returns>
        public async Task<IPagedList<ThreePlOrder>> SearchThreePlOrdersAsync(int threePlId = 0, int orderId = 0, int threePlOrderId = 0, List<int> tsIds = null, List<int> etsIds = null,
            bool onlyCancelledOrder = false, bool excludeCancelledOrder = false, bool excludeDeletedOrder = false,
            DateTime? cancelledDate = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _threePlOrderRepository.Table;

            //filter by threePl identifier
            if (threePlId > 0)
                query = query.Where(o => o.ThreePlId == threePlId);

            //filter by order
            if (orderId > 0)
            {
                query = from o in query
                        join oi in _threePlRepository.Table on o.ThreePlId equals oi.Id
                        where oi.OrderId == orderId
                        select o;

                query = query.Distinct();
            }

            //filter by threePl order
            if (threePlOrderId > 0)
                query = query.Where(o => o.ThreePlOrderId == threePlOrderId);

            //filter by 3PL status
            if (tsIds != null && tsIds.Any())
                query = query.Where(o => tsIds.Contains(o.ThreePlStutusId));

            //filter by only canceled order
            if (onlyCancelledOrder && cancelledDate.HasValue)
                query = query.Where(o => o.CancelledDate < cancelledDate.Value);

            //filter by only exclude canceled order
            if (excludeCancelledOrder)
                query = query.Where(o => o.CancelledDate == null);

            //filter by only exclude already synced order
            if (etsIds != null && etsIds.Any())
                query = query.Where(o => !etsIds.Contains(o.ThreePlStutusId));

            if (excludeDeletedOrder)
            {
                query = from o in query
                        join oit in _threePlRepository.Table on o.ThreePlId equals oit.Id
                        join oi in _orderRepository.Table on oit.OrderId equals oi.Id
                        where oi.Id == oit.OrderId && !oi.Deleted
                        select o;

                query = query.Distinct();
            }

            query = query.OrderByDescending(o => o.CreationDate);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Update the 3PL central Order
        /// </summary>
        /// <param name="threePlOrder">3PL central Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateThreePlOrderAsync(ThreePlOrder threePlOrder)
        {
            await _threePlOrderRepository.UpdateAsync(threePlOrder, false);
        }

        #endregion

        #region 3PL Shipping Method

        /// <summary>
        /// Delete a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod)
        {
            await _threePlShippingRepository.DeleteAsync(threePlShippingMethod, false);
        }

        /// <summary>
        /// Gets all 3PL Shipping Methods
        /// </summary>
        /// <param name="shippingMethod">Shipping method; null to load all 3PL orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax 3PL Shipping Method
        /// </returns>
        public async Task<IPagedList<ThreePlShippingMethod>> GetAllThreePlShippingMethodsAsync(string shippingMethod, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var tsm = await _threePlShippingRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(shippingMethod))
                    query = query.Where(c => c.ShippingMethod.Equals(shippingMethod));

                return query.OrderBy(c => c.ShippingMethod).ThenBy(c => c.ThreePlCarrier).ThenBy(c => c.Id);
            });

            var records = new PagedList<ThreePlShippingMethod>(tsm, pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Gets a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethodId">3PL Shipping Method identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL Shipping Method
        /// </returns>
        public async Task<ThreePlShippingMethod> GetThreePlShippingMethodByIdAsync(int threePlShippingMethodId)
        {
            return await _threePlShippingRepository.GetByIdAsync(threePlShippingMethodId);
        }

        /// <summary>
        /// Gets a 3PL Shipping Method
        /// </summary>
        /// <param name="shippingMethod">Default Shipping Method identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 3PL Shipping Method
        /// </returns>
        public async Task<ThreePlShippingMethod> GetThreePlShippingMethodByShippingMethodAsync(string shippingMethod)
        {
            return await _threePlShippingRepository.Table
                .FirstOrDefaultAsync(tsm => tsm.ShippingMethod.Equals(shippingMethod));
        }

        /// <summary>
        /// Insert a 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod)
        {
            await _threePlShippingRepository.InsertAsync(threePlShippingMethod, false);
        }

        /// <summary>
        /// Update the 3PL Shipping Method
        /// </summary>
        /// <param name="threePlShippingMethod">3PL Shipping Method</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateThreePlShippingMethodAsync(ThreePlShippingMethod threePlShippingMethod)
        {
            await _threePlShippingRepository.UpdateAsync(threePlShippingMethod, false);
        }

        #endregion

        #endregion
    }
}
