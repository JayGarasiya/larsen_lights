using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Reports.ChartsAndTables.Domain;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Reports.ChartsAndTables.Services
{
    /// <summary>
    /// Charts and tables service 
    /// </summary>
    public partial class ChartsAndTablesServices : IChartsAndTablesServices
    {
        #region constants

        protected const string ENCRYPTION_KEY = "MAKV2SPBNI99212";

        protected static readonly Regex _domainCleanRegex = new Regex(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/|www\.)|(\/.*)", RegexOptions.IgnoreCase);

        #endregion

        #region fields

        protected readonly ILogger _logger;
        protected readonly IWebHelper _webHelper;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IWidgetPluginManager _widgetPluginManager;
        protected readonly IOrderService _orderService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IRepository<Address> _addressRepository;
        protected readonly IRepository<Order> _orderRepository;
        protected readonly IRepository<OrderItem> _orderItemRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly IRepository<Vendor> _vendorRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<Customer> _customerRepository;
        protected readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
        protected readonly IRepository<CustomerRole> _customerRoleRepository;
        protected readonly IRepository<GenericAttribute> _genericAttributeRepository;
        protected readonly IRepository<Country> _countryRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public ChartsAndTablesServices(ILogger logger,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWidgetPluginManager widgetPluginManager,
            IOrderService orderService,
            IDateTimeHelper dateTimeHelper,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<Country> countryRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            ICategoryService categoryService)
        {
            _logger = logger;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _storeContext = storeContext;
            _widgetPluginManager = widgetPluginManager;
            _orderService = orderService;
            _dateTimeHelper = dateTimeHelper;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _manufacturerRepository = manufacturerRepository;
            _vendorRepository = vendorRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _customerRepository = customerRepository;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
            _customerRoleRepository = customerRoleRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _countryRepository = countryRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _categoryService = categoryService;
        }

        #endregion

        #region Utilities

        [Obsolete]
        protected string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        [Obsolete]
        protected async Task<string> Decrypt(string cipherText)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
            return cipherText;
        }


        /// <summary>
        /// Search order items
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="orderStatusId">Order status; null to load all records</param>
        /// <param name="paymentStatusId">Order payment status; null to load all records</param>
        /// <param name="countryId">Country identifier; 0 to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="customerRoleId">Customer Role identifier; 0 to load all records</param>
        /// <returns>Result query</returns>
        private IQueryable<OrderItem> SearchOrderItems(
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int orderStatusId = 0,
            int paymentStatusId = 0,
            int countryId = 0,
            string shippingMethod = null,
            string paymentMethod = null,
            int customerRoleId = 0)
        {
            var orderItems = from orderItem in _orderItemRepository.Table
                             join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             where (storeId == 0 || storeId == o.StoreId) &&
                                 (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                                 (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                                 (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                 (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                 (string.IsNullOrEmpty(shippingMethod) || shippingMethod == o.ShippingMethod) &&
                                 (string.IsNullOrEmpty(paymentMethod) || paymentMethod == o.PaymentMethodSystemName) &&
                                 !o.Deleted && !p.Deleted &&
                                 (vendorId == 0 || p.VendorId == vendorId)
                             select orderItem;

            //filter by country
            if (countryId > 0)
                orderItems = from oi in orderItems
                             join o in _orderRepository.Table on oi.OrderId equals o.Id
                             join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                             where
                                 countryId <= 0 || oba.CountryId == countryId
                             select oi;

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    orderItems =
                        from p in orderItems
                        join pc in productCategoryQuery on p.ProductId equals pc.ProductId
                        select p;
                }
            }

            if (manufacturerId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                             into p_pm
                             from pm in p_pm.DefaultIfEmpty()
                             where pm.ManufacturerId == manufacturerId
                             select orderItem;
            }

            //filter by customer role
            if (customerRoleId > 0)
                orderItems = from oi in orderItems
                             join o in _orderRepository.Table on oi.OrderId equals o.Id
                             join crm in _customerCustomerRoleMappingRepository.Table on o.CustomerId equals crm.CustomerId
                             where
                                 customerRoleId <= 0 || crm.CustomerRoleId == customerRoleId
                             select oi;

            return orderItems;
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="countryId">Country identifier; 0 to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="orderStatusId">Order status; null to load all records</param>
        /// <param name="paymentStatusId">Order payment status; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <returns>Result query</returns>
        private IQueryable<Order> SearchOrders(
            int storeId = 0,
            int countryId = 0,
            int customerRoleId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int orderStatusId = 0,
            int paymentStatusId = 0,
            string shippingMethod = null,
            string paymentMethod = null)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);

            //filter by date
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            //filter by order status
            if (orderStatusId > 0)
                query = query.Where(o => o.OrderStatusId == orderStatusId);

            //filter by payment status
            if (paymentStatusId > 0)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId);

            //filter by shipping method
            if (!string.IsNullOrEmpty(shippingMethod))
                query = query.Where(o => o.ShippingMethod == shippingMethod);

            //filter by payment method
            if (!string.IsNullOrEmpty(paymentMethod))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethod);

            //filter by country
            if (countryId > 0)
                query = from o in query
                        join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                        where
                            countryId <= 0 || oba.CountryId == countryId
                        select o;

            //filter by customer role
            if (customerRoleId > 0)
                query = from o in query
                        join crm in _customerCustomerRoleMappingRepository.Table on o.CustomerId equals crm.CustomerId
                        where
                            customerRoleId <= 0 || crm.CustomerRoleId == customerRoleId
                        select o;

            //filter by store
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);

            return query;
        }

        protected void PrepareGroupByTitle(IPagedList<ChartsAndTablesData> result, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var dayFormat = "MMM dd, yyyy";
            foreach (var reportLine in result)
            {
                var isCorrectDate =
                    DateTime.TryParseExact(reportLine.Title, "yyyy-MM-dd", null, DateTimeStyles.None, out var date) ||
                    DateTime.TryParseExact(reportLine.Title, "yyyy-M-d", null, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(reportLine.Title, "yyyy-MM", null, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(reportLine.Title, "yyyy-M", null, DateTimeStyles.None, out date);

                if (groupBy == ReportGroupByOptions.Week)
                {
                    if (!isCorrectDate)
                    {
                        var data = reportLine.Title.Replace("--", "-").Split("-").Select(int.Parse).ToList();
                        date = new DateTime(data[0], data[1], 1).AddDays((data[2] + 1) * -1);
                    }

                    var date1 = date.ToString(dayFormat);
                    var date2 = date.AddDays(6).ToString(dayFormat);
                    reportLine.Title = $"{date1} - {date2}";
                }
                else if (groupBy == ReportGroupByOptions.Quarter)
                {
                    var currQuarter = 0;
                    var data = reportLine.Title.Replace("--", "-").Split("-").Select(int.Parse).ToList();
                    if (!isCorrectDate)
                        date = new DateTime(data[0], 1, 1);

                    currQuarter = (data[1] + 1);

                    var dtFirstDay = new DateTime(date.Year, 3 * currQuarter - 2, 1);
                    var dtLastDay = dtFirstDay.AddMonths(3).AddDays(-1);
                    var date1 = dtFirstDay.ToString(dayFormat);
                    var date2 = dtLastDay.ToString(dayFormat);
                    reportLine.Title = $"{date1} - {date2}";
                }
                else if (isCorrectDate)
                {
                    var dateFormat = groupBy switch
                    {
                        ReportGroupByOptions.Day => dayFormat,
                        ReportGroupByOptions.Month => "MMMM, yyyy",
                        _ => ""
                    };

                    reportLine.Title = date.ToString(dateFormat);
                }
            }
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchProductMenuData(ProductReportEnum reportType = ProductReportEnum.ProductMenuProduct, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var query = SearchOrderItems(new List<int> { categoryId }, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                ProductReportEnum.ProductMenuProduct => (from orderItem in query
                                                         group orderItem by orderItem.ProductId into g
                                                         select new ChartsAndTablesData
                                                         {
                                                             Id = g.Key,
                                                             TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                             TotalQuantity = g.Sum(x => x.Quantity)
                                                         }),
                ProductReportEnum.ProductMenuProductAttribute => (from orderItem in query
                                                                  where !string.IsNullOrEmpty(orderItem.AttributeDescription)
                                                                  group orderItem by orderItem.ProductId into g
                                                                  select new ChartsAndTablesData
                                                                  {
                                                                      Id = g.Key,
                                                                      Title = g.FirstOrDefault().AttributeDescription,
                                                                      TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                      TotalQuantity = g.Sum(x => x.Quantity)
                                                                  }),
                ProductReportEnum.ProductMenuCategory => (from orderItem in query
                                                          join pc in _productCategoryRepository.Table on orderItem.ProductId equals pc.ProductId
                                                          where (categoryId == 0 || categoryId == pc.CategoryId)
                                                          group orderItem by pc.CategoryId into g
                                                          select new ChartsAndTablesData
                                                          {
                                                              Id = g.Key,
                                                              TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax)
                                                          }),
                ProductReportEnum.ProductMenuManufacturer => (from orderItem in query
                                                              join pm in _productManufacturerRepository.Table on orderItem.ProductId equals pm.ProductId
                                                              where (manufacturerId == 0 || manufacturerId == pm.ManufacturerId)
                                                              group orderItem by pm.ManufacturerId into g
                                                              select new ChartsAndTablesData
                                                              {
                                                                  Id = g.Key,
                                                                  TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax)
                                                              }),
                ProductReportEnum.ProductMenuVendor => (from orderItem in query
                                                        join p in _productRepository.Table on orderItem.ProductId equals p.Id
                                                        join v in _vendorRepository.Table on p.VendorId equals v.Id
                                                        where (vendorId == 0 || vendorId == p.VendorId)
                                                        group orderItem by v.Id into g
                                                        select new ChartsAndTablesData
                                                        {
                                                            Id = g.Key,
                                                            TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax)
                                                        }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchProductMenuAggrerator(ProductReportEnum reportType = ProductReportEnum.ProductMenuProduct, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var query = SearchProductMenuData(reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            var aggreratorReport =
                //group by products
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchCustomerMenuData(CustomerReportEnum reportType = CustomerReportEnum.CustomerMenuCustomer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrders(storeId, countryId, customerRoleId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                CustomerReportEnum.CustomerMenuCustomer => (from order in query
                                                            join c in _customerRepository.Table on order.CustomerId equals c.Id
                                                            group order by string.IsNullOrEmpty(c.Email) ? "Guest" : c.Email into g
                                                            select new ChartsAndTablesData
                                                            {
                                                                Title = g.Key,
                                                                TotalAmount = priceWithTax ? g.Sum(x => x.OrderSubtotalInclTax) : g.Sum(x => x.OrderSubtotalExclTax),
                                                                TotalQuantity = g.Count()
                                                            }),
                CustomerReportEnum.CustomerMenuCustomerRegistered => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                 group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderSubtotalInclTax) : order.Sum(orderItem => orderItem.OrderSubtotalExclTax),
                                                     TotalQuantity = 0
                                                 })
                                                .Union(from oq in query
                                                       join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                       where !string.IsNullOrEmpty(c.Email)
                                                       group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day}" into order
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = order.Key,
                                                           TotalAmount = 0,
                                                           TotalQuantity = order.Count()
                                                       }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                  group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day - (int)oq.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderSubtotalInclTax) : order.Sum(orderItem => orderItem.OrderSubtotalExclTax),
                                                      TotalQuantity = 0
                                                  })
                                                .Union(from oq in query
                                                       join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                       where !string.IsNullOrEmpty(c.Email)
                                                       group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day - (int)oq.CreatedOnUtc.DayOfWeek}" into order
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = order.Key,
                                                           TotalAmount = 0,
                                                           TotalQuantity = order.Count()
                                                       }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                   group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderSubtotalInclTax) : order.Sum(orderItem => orderItem.OrderSubtotalExclTax),
                                                       TotalQuantity = 0
                                                   })
                                                .Union(from oq in query
                                                       join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                       where !string.IsNullOrEmpty(c.Email)
                                                       group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}" into order
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = order.Key,
                                                           TotalAmount = 0,
                                                           TotalQuantity = order.Count()
                                                       }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                     group oq by $"{oq.CreatedOnUtc.Year}-{(oq.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderSubtotalInclTax) : order.Sum(orderItem => orderItem.OrderSubtotalExclTax),
                                                         TotalQuantity = 0
                                                     })
                                                    .Union(from oq in query
                                                           join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                           where !string.IsNullOrEmpty(c.Email)
                                                           group oq by $"{oq.CreatedOnUtc.Year}-{(oq.CreatedOnUtc.Month - 1) / 3}" into order
                                                           select new ChartsAndTablesData
                                                           {
                                                               Title = order.Key,
                                                               TotalAmount = 0,
                                                               TotalQuantity = order.Count()
                                                           }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                  group oq by $"{oq.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderSubtotalInclTax) : order.Sum(orderItem => orderItem.OrderSubtotalExclTax),
                                                      TotalQuantity = 0
                                                  })
                                                .Union(from oq in query
                                                       join c in _customerRepository.Table on oq.CustomerId equals c.Id
                                                       where !string.IsNullOrEmpty(c.Email)
                                                       group oq by $"{oq.CreatedOnUtc.Year}" into order
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = order.Key,
                                                           TotalAmount = 0,
                                                           TotalQuantity = order.Count()
                                                       }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                                      group order by order.Title into g
                                                                      orderby g.Key descending
                                                                      select new ChartsAndTablesData
                                                                      {
                                                                          Title = g.Key,
                                                                          TotalAmount = g.Sum(x => x.TotalAmount),
                                                                          TotalQuantity = g.Sum(x => x.TotalQuantity)
                                                                      }),
                CustomerReportEnum.CustomerMenuCustomerGender => (from order in query
                                                                  join c in _customerRepository.Table on order.CustomerId equals c.Id
                                                                  join g in _genericAttributeRepository.Table on c.Id equals g.EntityId into ou
                                                                  from oq in ou.DefaultIfEmpty()
                                                                  where oq.KeyGroup.Equals("Customer") && oq.Key.Equals("Gender")
                                                                  group order by string.IsNullOrEmpty(oq.Value) ? "(empty)" : oq.Value into g
                                                                  select new ChartsAndTablesData
                                                                  {
                                                                      Title = g.Key,
                                                                      TotalAmount = priceWithTax ? g.Sum(x => x.OrderSubtotalInclTax) : g.Sum(x => x.OrderSubtotalExclTax),
                                                                      TotalQuantity = g.Count()
                                                                  }),
                CustomerReportEnum.CustomerMenuCustomerRole => (from order in query
                                                                join c in _customerRepository.Table on order.CustomerId equals c.Id
                                                                join crm in _customerCustomerRoleMappingRepository.Table on c.Id equals crm.CustomerId
                                                                join cr in _customerRoleRepository.Table on crm.CustomerRoleId equals cr.Id into ou
                                                                from oq in ou.DefaultIfEmpty()
                                                                group order by oq.Name into g
                                                                select new ChartsAndTablesData
                                                                {
                                                                    Title = g.Key,
                                                                    TotalAmount = priceWithTax ? g.Sum(x => x.OrderSubtotalInclTax) : g.Sum(x => x.OrderSubtotalExclTax),
                                                                    TotalQuantity = g.Count()
                                                                }),
                CustomerReportEnum.CustomerMenuCountry => (from order in query
                                                           join a in _addressRepository.Table on order.BillingAddressId equals a.Id
                                                           join c in _countryRepository.Table on a.CountryId equals c.Id into ou
                                                           from oq in ou.DefaultIfEmpty()
                                                           group order by oq.Name into g
                                                           select new ChartsAndTablesData
                                                           {
                                                               Title = g.Key,
                                                               TotalAmount = priceWithTax ? g.Sum(x => x.OrderSubtotalInclTax) : g.Sum(x => x.OrderSubtotalExclTax),
                                                               TotalQuantity = g.Count()
                                                           }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchCustomerMenuAggrerator(CustomerReportEnum reportType = CustomerReportEnum.CustomerMenuCustomer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchCustomerMenuData(reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by order
                await (from order in query
                       group order by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchOrderMenuData(OrderReportEnum reportType = OrderReportEnum.OrderMenuOrder, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            if (reportType.Equals(OrderReportEnum.OrderMenuOrderItem))
            {
                var query = SearchOrderItems(null, 0, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId, countryId, shippingMethod, paymentMethod);

                return (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     TotalQuantity = order.Count()
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Count()
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                       TotalQuantity = order.Count()
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                         TotalQuantity = order.Count()
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Count()
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                        group order by order.Title into g
                        select new ChartsAndTablesData
                        {
                            Title = g.Key,
                            TotalAmount = g.Sum(x => x.TotalAmount),
                            TotalQuantity = g.Sum(x => x.TotalQuantity)
                        });
            }
            else
            {
                var query = SearchOrders(storeId, countryId, 0, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId, shippingMethod, paymentMethod);
                var bsReport = reportType switch
                {
                    OrderReportEnum.OrderMenuOrder => (from order in groupBy switch
                    {
                        ReportGroupByOptions.Day => (from oq in query
                                                     group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderTotal) : order.Sum(orderItem => (orderItem.OrderTotal - orderItem.OrderTax)),
                                                         TotalQuantity = order.Count()
                                                     }),
                        ReportGroupByOptions.Week => (from oq in query
                                                      group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}-{oq.CreatedOnUtc.Day - (int)oq.CreatedOnUtc.DayOfWeek}" into order
                                                      select new ChartsAndTablesData
                                                      {
                                                          Title = order.Key,
                                                          TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderTotal) : order.Sum(orderItem => (orderItem.OrderTotal - orderItem.OrderTax)),
                                                          TotalQuantity = order.Count()
                                                      }),
                        ReportGroupByOptions.Month => (from oq in query
                                                       group oq by $"{oq.CreatedOnUtc.Year}-{oq.CreatedOnUtc.Month}" into order
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = order.Key,
                                                           TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderTotal) : order.Sum(orderItem => (orderItem.OrderTotal - orderItem.OrderTax)),
                                                           TotalQuantity = order.Count()
                                                       }),
                        ReportGroupByOptions.Quarter => (from oq in query
                                                         group oq by $"{oq.CreatedOnUtc.Year}-{(oq.CreatedOnUtc.Month - 1) / 3}" into order
                                                         select new ChartsAndTablesData
                                                         {
                                                             Title = order.Key,
                                                             TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderTotal) : order.Sum(orderItem => (orderItem.OrderTotal - orderItem.OrderTax)),
                                                             TotalQuantity = order.Count()
                                                         }),
                        ReportGroupByOptions.Year => (from oq in query
                                                      group oq by $"{oq.CreatedOnUtc.Year}" into order
                                                      select new ChartsAndTablesData
                                                      {
                                                          Title = order.Key,
                                                          TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.OrderTotal) : order.Sum(orderItem => (orderItem.OrderTotal - orderItem.OrderTax)),
                                                          TotalQuantity = order.Count()
                                                      }),
                        _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                    }
                                                       group order by order.Title into g
                                                       select new ChartsAndTablesData
                                                       {
                                                           Title = g.Key,
                                                           TotalAmount = g.Sum(x => x.TotalAmount),
                                                           TotalQuantity = g.Sum(x => x.TotalQuantity)
                                                       }),
                    OrderReportEnum.OrderMenuShippingMethod => (from order in query
                                                                group order by string.IsNullOrEmpty(order.ShippingMethod) ? "(empty)" : order.ShippingMethod into g
                                                                select new ChartsAndTablesData
                                                                {
                                                                    Title = g.Key,
                                                                    TotalAmount = priceWithTax ? g.Sum(x => x.OrderTotal) : g.Sum(x => (x.OrderTotal - x.OrderTax)),
                                                                    TotalQuantity = g.Count()
                                                                }),
                    OrderReportEnum.OrderMenuPaymentMethod => (from order in query
                                                               group order by string.IsNullOrEmpty(order.PaymentMethodSystemName.Replace("Payments.", string.Empty)) ? "(empty)" : order.PaymentMethodSystemName.Replace("Payments.", string.Empty) into g
                                                               select new ChartsAndTablesData
                                                               {
                                                                   Title = g.Key,
                                                                   TotalAmount = priceWithTax ? g.Sum(x => x.OrderTotal) : g.Sum(x => (x.OrderTotal - x.OrderTax)),
                                                                   TotalQuantity = g.Count()
                                                               }),
                    _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
                };

                return bsReport;
            }
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchOrderMenuAggrerator(OrderReportEnum reportType = OrderReportEnum.OrderMenuOrder, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderMenuData(reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax, groupBy);

            var aggreratorReport =
                //group by order
                await (from order in query
                       group order by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }


        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private IQueryable<TableReport> SearchTableReport(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var query = SearchOrderItems(new List<int> { categoryId }, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = from oi in query
                           join o in _orderRepository.Table on oi.OrderId equals o.Id
                           join oc in _customerRepository.Table on o.CustomerId equals oc.Id
                           join a in _addressRepository.Table on o.BillingAddressId equals a.Id
                           join c in _countryRepository.Table on a.CountryId equals c.Id
                           group new
                           {
                               oi.OrderId,
                               o.CustomerId,
                               CustomerEmail = string.IsNullOrEmpty(oc.Email) ? a.Email : oc.Email,
                               oi.ProductId,
                               oi.AttributeDescription,
                               oi.Quantity,
                               oi.UnitPriceExclTax,
                               oi.PriceExclTax,
                               o.PaymentMethodSystemName,
                               o.ShippingMethod,
                               BillingCountry = c.Name,
                               BillingCity = a.City,
                               YearMonth = $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}"
                           } by new
                           {
                               oi.OrderId,
                               o.CustomerId,
                               CustomerEmail = string.IsNullOrEmpty(oc.Email) ? a.Email : oc.Email,
                               oi.ProductId,
                               oi.AttributeDescription,
                               oi.Quantity,
                               oi.UnitPriceExclTax,
                               oi.PriceExclTax,
                               o.PaymentMethodSystemName,
                               o.ShippingMethod,
                               BillingCountry = c.Name,
                               BillingCity = a.City,
                               YearMonth = $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}"
                           } into g
                           select new TableReport
                           {
                               OrderId = g.Key.OrderId,
                               CustomerId = g.Key.CustomerId,
                               CustomerEmail = g.Key.CustomerEmail,
                               ProductId = g.Key.ProductId,
                               Attributes = g.Key.AttributeDescription,
                               Quantity = g.Key.Quantity,
                               UnitPriceExclTax = g.Key.UnitPriceExclTax,
                               LineTotalExclTax = g.Key.PriceExclTax,
                               PaymentMethod = g.Key.PaymentMethodSystemName,
                               ShippingMethod = g.Key.ShippingMethod,
                               BillingCountry = g.Key.BillingCountry,
                               BillingCity = g.Key.BillingCity,
                               YearMonth = g.Key.YearMonth
                           };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchTableReportAggrerator(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0)
        {
            var query = SearchTableReport(createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId);

            var aggreratorReport =
                //group by products
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.Quantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.LineTotalExclTax),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchProductDetailData(int productId, ProductReportEnum reportType = ProductReportEnum.ProductDetailProduct, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderItems(null, 0, storeId, 0, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId, countryId, customerRoleId: customerRoleId);

            var bsReport = reportType switch
            {
                ProductReportEnum.ProductDetailProduct => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 where oq.ProductId == productId
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  where oq.ProductId == productId
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   where oq.ProductId == productId
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                       TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     where oq.ProductId == productId
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                         TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  where oq.ProductId == productId
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                           group order by order.Title into g
                                                           select new ChartsAndTablesData
                                                           {
                                                               Title = g.Key,
                                                               TotalAmount = g.Sum(x => x.TotalAmount),
                                                               TotalQuantity = g.Sum(x => x.TotalQuantity)
                                                           }),
                ProductReportEnum.ProductDetailProductAttribute => (from oi in query
                                                                    join pac in _productAttributeCombinationRepository.Table on oi.ProductId equals pac.ProductId
                                                                    where !string.IsNullOrEmpty(oi.AttributeDescription) && oi.AttributesXml == pac.AttributesXml && oi.ProductId == productId
                                                                    group oi by $"{pac.Sku} - {oi.AttributeDescription}" into g
                                                                    select new ChartsAndTablesData
                                                                    {
                                                                        Title = g.Key,
                                                                        TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                        TotalQuantity = g.Sum(x => x.Quantity)
                                                                    }),
                ProductReportEnum.ProductDetailCustomer => (from oi in query
                                                            join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                            join c in _customerRepository.Table on o.CustomerId equals c.Id
                                                            where oi.ProductId == productId
                                                            group oi by string.IsNullOrEmpty(c.Email) ? "Guest" : c.Email into g
                                                            select new ChartsAndTablesData
                                                            {
                                                                Title = g.Key,
                                                                TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                TotalQuantity = g.Sum(x => x.Quantity)
                                                            }),
                ProductReportEnum.ProductDetailCountry => (from oi in query
                                                           join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                           join a in _addressRepository.Table on o.BillingAddressId equals a.Id
                                                           where oi.ProductId == productId
                                                           group oi by new
                                                           {
                                                               CountryId = a.CountryId.HasValue ? a.CountryId.Value : 0,
                                                               StateId = a.StateProvinceId.HasValue ? a.StateProvinceId.Value : 0
                                                           } into g
                                                           select new ChartsAndTablesData
                                                           {
                                                               Id = g.Key.CountryId,
                                                               Title = g.Key.StateId.ToString(),
                                                               TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                               TotalQuantity = g.Sum(x => x.Quantity)
                                                           }),
                ProductReportEnum.ProductDetailGlobal => (from oq in query
                                                          where oq.ProductId == productId
                                                          group oq by 1 into g
                                                          select g).Distinct()
                                                          .Select(g =>
                                                           new ChartsAndTablesData
                                                           {
                                                               Title = "Total Sales",
                                                               TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                           })
                                                          .Union((from oq in query
                                                                  where oq.ProductId == productId
                                                                  group oq by 1 into g
                                                                  select g).Distinct()
                                                          .Select(g => new ChartsAndTablesData
                                                          {
                                                              Title = "Total Quantity",
                                                              TotalAmount = g.Sum(x => x.Quantity)
                                                          }))
                                                          .Union(from oq in query
                                                                 where oq.ProductId == productId
                                                                 group oq by 1 into g
                                                                 select new ChartsAndTablesData
                                                                 {
                                                                     Title = "Quantity Order",
                                                                     TotalAmount = g.Select(p => p.OrderId).Distinct().Count()
                                                                 }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchProductDetailAggrerator(int productId, ProductReportEnum reportType = ProductReportEnum.ProductMenuProduct, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchProductDetailData(productId, reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by products
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchCustomerDetailData(int customerId, CustomerReportEnum reportType = CustomerReportEnum.CustomerDetailCustomer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderItems(new List<int> { categoryId }, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                CustomerReportEnum.CustomerDetailCustomer => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 where o.CustomerId == customerId
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  where o.CustomerId == customerId
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   where o.CustomerId == customerId
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                       TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     where o.CustomerId == customerId
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                         TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  where o.CustomerId == customerId
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                      TotalQuantity = order.Select(p => p.OrderId).Distinct().Count()
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                              group order by order.Title into g
                                                              select new ChartsAndTablesData
                                                              {
                                                                  Title = g.Key,
                                                                  TotalAmount = g.Sum(x => x.TotalAmount),
                                                                  TotalQuantity = g.Sum(x => x.TotalQuantity)
                                                              }),
                CustomerReportEnum.CustomerDetailProduct => (from oi in query
                                                             join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                             where o.CustomerId == customerId
                                                             group oi by oi.ProductId into g
                                                             select new ChartsAndTablesData
                                                             {
                                                                 Id = g.Key,
                                                                 TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                 TotalQuantity = g.Sum(x => x.Quantity)
                                                             }),
                CustomerReportEnum.CustomerDetailCategory => (from oi in query
                                                              join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                              join pc in _productCategoryRepository.Table on oi.ProductId equals pc.ProductId
                                                              where o.CustomerId == customerId
                                                              group oi by pc.CategoryId into g
                                                              select new ChartsAndTablesData
                                                              {
                                                                  Id = g.Key,
                                                                  TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                  TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                              }),
                CustomerReportEnum.CustomerDetailManufacturer => (from oi in query
                                                                  join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                                  join pm in _productManufacturerRepository.Table on oi.ProductId equals pm.ProductId
                                                                  where o.CustomerId == customerId
                                                                  group oi by pm.ManufacturerId into g
                                                                  select new ChartsAndTablesData
                                                                  {
                                                                      Id = g.Key,
                                                                      TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                      TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                                  }),
                CustomerReportEnum.CustomerDetailVendor => (from oi in query
                                                            join o in _orderRepository.Table on oi.OrderId equals o.Id
                                                            join p in _productRepository.Table on oi.ProductId equals p.Id
                                                            join v in _vendorRepository.Table on p.VendorId equals v.Id
                                                            where o.CustomerId == customerId
                                                            group oi by v.Id into g
                                                            select new ChartsAndTablesData
                                                            {
                                                                Id = g.Key,
                                                                TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                            }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchCustomerDetailAggrerator(int customerId, CustomerReportEnum reportType = CustomerReportEnum.CustomerDetailCustomer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchCustomerDetailData(customerId, reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by customers
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="categoryIds">Category identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchCategoryDetailData(IList<int> categoryIds, CategoryReportEnum reportType = CategoryReportEnum.CategoryDetailCategory, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderItems(categoryIds, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                CategoryReportEnum.CategoryDetailCategory => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                              group order by order.Title into g
                                                              select new ChartsAndTablesData
                                                              {
                                                                  Title = g.Key,
                                                                  TotalAmount = g.Sum(x => x.TotalAmount),
                                                              }),
                CategoryReportEnum.CategoryDetailProduct => (from oi in query
                                                             group oi by oi.ProductId into g
                                                             select new ChartsAndTablesData
                                                             {
                                                                 Id = g.Key,
                                                                 TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                 TotalQuantity = g.Sum(x => x.Quantity)
                                                             }),
                CategoryReportEnum.CategoryDetailManufacturer => (from oi in query
                                                                  join pm in _productManufacturerRepository.Table on oi.ProductId equals pm.ProductId
                                                                  group oi by pm.ManufacturerId into g
                                                                  select new ChartsAndTablesData
                                                                  {
                                                                      Id = g.Key,
                                                                      TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                      TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                                  }),
                CategoryReportEnum.CategoryDetailVendor => (from oi in query
                                                            join p in _productRepository.Table on oi.ProductId equals p.Id
                                                            join v in _vendorRepository.Table on p.VendorId equals v.Id
                                                            group oi by v.Id into g
                                                            select new ChartsAndTablesData
                                                            {
                                                                Id = g.Key,
                                                                TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                            }),
                CategoryReportEnum.CategoryDetailShare => (from oi in query
                                                           join pc in _productCategoryRepository.Table on oi.ProductId equals pc.ProductId
                                                           group oi by pc.CategoryId into g
                                                           select new ChartsAndTablesData
                                                           {
                                                               Id = g.Key,
                                                               TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                           }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="categoryIds">Category identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchCategoryDetailAggrerator(IList<int> categoryIds, CategoryReportEnum reportType = CategoryReportEnum.CategoryDetailCategory, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchCategoryDetailData(categoryIds, reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by customers
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchManufacturerDetailData(int manufacturerId, ManufacturerReportEnum reportType = ManufacturerReportEnum.ManufacturerDetailManufacturer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderItems(new List<int> { categoryId }, manufacturerId, storeId, 0, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                ManufacturerReportEnum.ManufacturerDetailManufacturer => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                                          group order by order.Title into g
                                                                          select new ChartsAndTablesData
                                                                          {
                                                                              Title = g.Key,
                                                                              TotalAmount = g.Sum(x => x.TotalAmount),
                                                                          }),
                ManufacturerReportEnum.ManufacturerDetailProduct => (from oi in query
                                                                     group oi by oi.ProductId into g
                                                                     select new ChartsAndTablesData
                                                                     {
                                                                         Id = g.Key,
                                                                         TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                         TotalQuantity = g.Sum(x => x.Quantity)
                                                                     }),
                ManufacturerReportEnum.ManufacturerDetailCategory => (from oi in query
                                                                      join pc in _productCategoryRepository.Table on oi.ProductId equals pc.ProductId
                                                                      group oi by pc.CategoryId into g
                                                                      select new ChartsAndTablesData
                                                                      {
                                                                          Id = g.Key,
                                                                          TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                          TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                                      }),
                ManufacturerReportEnum.ManufacturerDetailShare => (from oi in query
                                                                   join pm in _productManufacturerRepository.Table on oi.ProductId equals pm.ProductId
                                                                   group oi by pm.ManufacturerId into g
                                                                   select new ChartsAndTablesData
                                                                   {
                                                                       Id = g.Key,
                                                                       TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                   }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchManufacturerDetailAggrerator(int manufacturerId, ManufacturerReportEnum reportType = ManufacturerReportEnum.ManufacturerDetailManufacturer, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchManufacturerDetailData(manufacturerId, reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by customers
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private IQueryable<ChartsAndTablesData> SearchVendorDetailData(int vendorId, VendorReportEnum reportType = VendorReportEnum.VendorDetailVendor, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchOrderItems(new List<int> { categoryId }, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, orderStatusId, paymentStatusId);

            var bsReport = reportType switch
            {
                VendorReportEnum.VendorDetailVendor => (from order in groupBy switch
                {
                    ReportGroupByOptions.Day => (from oq in query
                                                 join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                 group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day}" into order
                                                 select new ChartsAndTablesData
                                                 {
                                                     Title = order.Key,
                                                     TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                 }),
                    ReportGroupByOptions.Week => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}-{o.CreatedOnUtc.Day - (int)o.CreatedOnUtc.DayOfWeek}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    ReportGroupByOptions.Month => (from oq in query
                                                   join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                   group oq by $"{o.CreatedOnUtc.Year}-{o.CreatedOnUtc.Month}" into order
                                                   select new ChartsAndTablesData
                                                   {
                                                       Title = order.Key,
                                                       TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                   }),
                    ReportGroupByOptions.Quarter => (from oq in query
                                                     join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                     group oq by $"{o.CreatedOnUtc.Year}-{(o.CreatedOnUtc.Month - 1) / 3}" into order
                                                     select new ChartsAndTablesData
                                                     {
                                                         Title = order.Key,
                                                         TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                     }),
                    ReportGroupByOptions.Year => (from oq in query
                                                  join o in _orderRepository.Table on oq.OrderId equals o.Id
                                                  group oq by $"{o.CreatedOnUtc.Year}" into order
                                                  select new ChartsAndTablesData
                                                  {
                                                      Title = order.Key,
                                                      TotalAmount = priceWithTax ? order.Sum(orderItem => orderItem.PriceInclTax) : order.Sum(orderItem => orderItem.PriceExclTax),
                                                  }),
                    _ => throw new ArgumentException("Wrong grouBy parameter", nameof(groupBy)),
                }
                                                        group order by order.Title into g
                                                        select new ChartsAndTablesData
                                                        {
                                                            Title = g.Key,
                                                            TotalAmount = g.Sum(x => x.TotalAmount),
                                                        }),
                VendorReportEnum.VendorDetailProduct => (from oi in query
                                                         group oi by oi.ProductId into g
                                                         select new ChartsAndTablesData
                                                         {
                                                             Id = g.Key,
                                                             TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                             TotalQuantity = g.Sum(x => x.Quantity)
                                                         }),
                VendorReportEnum.VendorDetailCategory => (from oi in query
                                                          join pc in _productCategoryRepository.Table on oi.ProductId equals pc.ProductId
                                                          group oi by pc.CategoryId into g
                                                          select new ChartsAndTablesData
                                                          {
                                                              Id = g.Key,
                                                              TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                              TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                          }),
                VendorReportEnum.VendorDetailManufacturer => (from oi in query
                                                              join pm in _productManufacturerRepository.Table on oi.ProductId equals pm.ProductId
                                                              group oi by pm.ManufacturerId into g
                                                              select new ChartsAndTablesData
                                                              {
                                                                  Id = g.Key,
                                                                  TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                                  TotalQuantity = g.Select(p => p.OrderId).Distinct().Count()
                                                              }),
                VendorReportEnum.VendorDetailShare => (from oi in query
                                                       join p in _productRepository.Table on oi.ProductId equals p.Id
                                                       join v in _vendorRepository.Table on p.VendorId equals v.Id
                                                       group oi by v.Id into g
                                                       select new ChartsAndTablesData
                                                       {
                                                           Id = g.Key,
                                                           TotalAmount = priceWithTax ? g.Sum(x => x.PriceInclTax) : g.Sum(x => x.PriceExclTax),
                                                       }),
                _ => throw new ArgumentException("Wrong report type parameter", nameof(reportType)),
            };

            return bsReport;
        }

        /// <summary>
        /// Search charts and tables data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="reportType">Report identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>Result query</returns>
        private async Task<AggreratorLine> SearchVendorDetailAggrerator(int vendorId, VendorReportEnum reportType = VendorReportEnum.VendorDetailVendor, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, ReportGroupByOptions groupBy = ReportGroupByOptions.Month)
        {
            var query = SearchVendorDetailData(vendorId, reportType, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax, groupBy);

            var aggreratorReport =
                //group by customers
                await (from orderItem in query
                       group orderItem by 1 into result
                       select new AggreratorLine
                       {
                           TotalQuantity = result.Sum(orderItem => orderItem.TotalQuantity),
                           AverageQuantity = (decimal)result.Average(orderItem => orderItem.TotalQuantity),
                           MaxQuantity = result.Max(orderItem => orderItem.TotalQuantity),
                           MinQuantity = result.Min(orderItem => orderItem.TotalQuantity),
                           TotalSalesValue = result.Sum(orderItem => orderItem.TotalAmount),
                           AverageSalesValue = result.Average(orderItem => orderItem.TotalAmount),
                           MaxSalesValue = result.Max(orderItem => orderItem.TotalAmount),
                           MinSalesValue = result.Min(orderItem => orderItem.TotalAmount),
                       }).FirstOrDefaultAsync();

            aggreratorReport ??= new AggreratorLine
            {
                TotalQuantity = 0,
                AverageQuantity = decimal.Zero,
                MaxQuantity = 0,
                MinQuantity = 0,
                TotalSalesValue = decimal.Zero,
                AverageSalesValue = decimal.Zero,
                MaxSalesValue = decimal.Zero,
                MinSalesValue = decimal.Zero
            };

            return aggreratorReport;
        }

        #endregion

        #region License

        [Obsolete]
        public async Task<int> CheckLicenseKeyValidAsync(string licenseKey)
        {
            try
            {
                string[] decryptdomainname = null;
                string decryptedKey = await Decrypt(licenseKey);

                string primaryDomainName = "";

                decryptdomainname = decryptedKey.Split(',');

                string domainName = string.Empty;

                if (decryptdomainname[0].Contains("https://www."))
                {
                    domainName = decryptdomainname[0].Replace("https://www.", string.Empty);
                }
                else if (decryptdomainname[0].Contains("https://"))
                {
                    domainName = decryptdomainname[0].Replace("https://", string.Empty);
                }
                else if (decryptdomainname[0].Contains("www."))
                {
                    domainName = decryptdomainname[0].Replace("www.", string.Empty);
                }
                else
                {
                    domainName = decryptdomainname[0].Replace("http://", string.Empty);
                }

                double days = 0;
                if (decryptdomainname[1] != "Full")
                {
                    DateTime activeDate;
                    var isTrue = DateTime.TryParseExact(decryptdomainname[3].Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out activeDate);

                    DateTime dt = DateTime.Now;
                    DateTime serverDate = dt.AddHours(11).AddMinutes(30);

                    days = (activeDate - serverDate).TotalDays;
                }

                primaryDomainName = _webHelper.GetStoreLocation();

                var objstoreURL = new Uri(primaryDomainName);
                string host = objstoreURL.Authority;

                if (host.StartsWith("www."))
                    primaryDomainName = host.Substring(4);
                else
                    primaryDomainName = host;
                string currentDomain = objstoreURL.Authority;

                if (primaryDomainName.Contains(domainName) && decryptdomainname[2] == "Reports-ChartAndTable")
                {
                    return 100; // perfect
                }
                else if (primaryDomainName.Contains(domainName) && decryptdomainname[2] == "Reports-ChartAndTable" && days > 0)
                {
                    return 101; // plugin is in trial mode.
                }
                else if (domainName == primaryDomainName && days <= 0)
                {
                    return 103; //trial license is expired
                }
                else
                {
                    return 102; // domain name mismatch
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
            return 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether the plugin is active for the current customer and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<bool> PluginActiveAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            return await _widgetPluginManager.IsPluginActiveAsync(ChartsAndTablesDefaults.SystemName, customer, store?.Id ?? 0);
        }

        #region Dashboard 

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardWeekStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax)
        {
            var nowDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

            var currentFirst = nowDt.AddDays(-1 * (int)nowDt.DayOfWeek);
            var previousFirst = currentFirst.AddDays(-7);

            var previousWeek = Enumerable.Range(0, 7)
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(previousFirst.AddDays(offset), timeZone),
                                WeekOfDay = previousFirst.AddDays(offset).DayOfWeek
                            }).ToList();

            var currentWeek = Enumerable.Range(0, 1 + (int)nowDt.DayOfWeek)
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(currentFirst.AddDays(offset), timeZone),
                                WeekOfDay = currentFirst.AddDays(offset).DayOfWeek
                            }).ToList();

            var result = new List<object>();
            if (vendorId > 0)
            {
                var query = await (from d in previousWeek
                                   from r in (from oi in _orderItemRepository.Table
                                              join o in _orderRepository.Table on oi.OrderId equals o.Id
                                              join p in _productRepository.Table on oi.ProductId equals p.Id
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted && !p.Deleted &&
                                              (vendorId == 0 || p.VendorId == vendorId)
                                              select oi).DefaultIfEmpty()
                                   group r by d.WeekOfDay into orderItem
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = orderItem.Key,
                                       TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                                   })
                        .Union(from d in currentWeek
                               from r in (from oi in _orderItemRepository.Table
                                          join o in _orderRepository.Table on oi.OrderId equals o.Id
                                          join p in _productRepository.Table on oi.ProductId equals p.Id
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted && !p.Deleted &&
                                          (vendorId == 0 || p.VendorId == vendorId)
                                          select oi).DefaultIfEmpty()
                               group r by d.WeekOfDay into orderItem
                               select new
                               {
                                   Interval = "Current",
                                   Day = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousWeek.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.WeekOfDay,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.WeekOfDay) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentWeek.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.WeekOfDay,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.WeekOfDay) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }
            else
            {
                var query = await (from d in previousWeek
                                   from r in (from o in _orderRepository.Table
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted
                                              select o).DefaultIfEmpty()
                                   group r by d.WeekOfDay into order
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = order.Key,
                                       TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                                   })
                        .Union(from d in currentWeek
                               from r in (from o in _orderRepository.Table
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted
                                          select o).DefaultIfEmpty()
                               group r by d.WeekOfDay into order
                               select new
                               {
                                   Interval = "Current",
                                   Day = order.Key,
                                   TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousWeek.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.WeekOfDay,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.WeekOfDay) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentWeek.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.WeekOfDay,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.WeekOfDay) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }

            return result;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax)
        {
            var nowDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

            var currentFirst = nowDt;
            var previousFirst = currentFirst.AddMonths(-1);

            var previousMonth = Enumerable.Range(0, DateTime.DaysInMonth(previousFirst.Year, previousFirst.Month))
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(previousFirst.AddDays(offset), timeZone),
                                DayOfMonth = previousFirst.AddDays(offset).Day
                            }).ToList();

            var currentMonth = Enumerable.Range(0, DateTime.Now.Day)
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(currentFirst.AddDays(offset), timeZone),
                                DayOfMonth = currentFirst.AddDays(offset).Day
                            }).ToList();

            var result = new List<object>();
            if (vendorId > 0)
            {
                var query = await (from d in previousMonth
                                   from r in (from oi in _orderItemRepository.Table
                                              join o in _orderRepository.Table on oi.OrderId equals o.Id
                                              join p in _productRepository.Table on oi.ProductId equals p.Id
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted && !p.Deleted &&
                                              (vendorId == 0 || p.VendorId == vendorId)
                                              select oi).DefaultIfEmpty()
                                   group r by d.DayOfMonth into orderItem
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = orderItem.Key,
                                       TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                                   })
                        .Union(from d in currentMonth
                               from r in (from oi in _orderItemRepository.Table
                                          join o in _orderRepository.Table on oi.OrderId equals o.Id
                                          join p in _productRepository.Table on oi.ProductId equals p.Id
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted && !p.Deleted &&
                                          (vendorId == 0 || p.VendorId == vendorId)
                                          select oi).DefaultIfEmpty()
                               group r by d.DayOfMonth into orderItem
                               select new
                               {
                                   Interval = "Current",
                                   Day = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousMonth.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.DayOfMonth,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.DayOfMonth) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentMonth.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.DayOfMonth,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.DayOfMonth) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }
            else
            {
                var query = await (from d in previousMonth
                                   from r in (from o in _orderRepository.Table
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted
                                              select o).DefaultIfEmpty()
                                   group r by d.DayOfMonth into order
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = order.Key,
                                       TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                                   })
                        .Union(from d in currentMonth
                               from r in (from o in _orderRepository.Table
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddDays(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted
                                          select o).DefaultIfEmpty()
                               group r by d.DayOfMonth into order
                               select new
                               {
                                   Interval = "Current",
                                   Day = order.Key,
                                   TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousMonth.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.DayOfMonth,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.DayOfMonth) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentMonth.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.DayOfMonth,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.DayOfMonth) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }

            return result;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardYearStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, bool priceWithTax)
        {
            var nowDt = new DateTime(DateTime.Now.Year, 1, 1);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

            var currentFirst = nowDt;
            var previousFirst = currentFirst.AddYears(-1);

            var previousMonth = Enumerable.Range(0, 12)
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(previousFirst.AddMonths(offset), timeZone),
                                MonthOfYear = previousFirst.AddMonths(offset).Month
                            }).ToList();

            var currentMonth = Enumerable.Range(0, DateTime.Now.Month)
                            .Select(offset => new
                            {
                                UtcTime = _dateTimeHelper.ConvertToUtcTime(currentFirst.AddMonths(offset), timeZone),
                                MonthOfYear = currentFirst.AddMonths(offset).Month
                            }).ToList();

            var result = new List<object>();
            if (vendorId > 0)
            {
                var query = await (from d in previousMonth
                                   from r in (from oi in _orderItemRepository.Table
                                              join o in _orderRepository.Table on oi.OrderId equals o.Id
                                              join p in _productRepository.Table on oi.ProductId equals p.Id
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted && !p.Deleted &&
                                              (vendorId == 0 || p.VendorId == vendorId)
                                              select oi).DefaultIfEmpty()
                                   group r by d.MonthOfYear into orderItem
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = orderItem.Key,
                                       TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                                   })
                        .Union(from d in currentMonth
                               from r in (from oi in _orderItemRepository.Table
                                          join o in _orderRepository.Table on oi.OrderId equals o.Id
                                          join p in _productRepository.Table on oi.ProductId equals p.Id
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted && !p.Deleted &&
                                          (vendorId == 0 || p.VendorId == vendorId)
                                          select oi).DefaultIfEmpty()
                               group r by d.MonthOfYear into orderItem
                               select new
                               {
                                   Interval = "Current",
                                   Day = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousMonth.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentMonth.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }
            else
            {
                var query = await (from d in previousMonth
                                   from r in (from o in _orderRepository.Table
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted
                                              select o).DefaultIfEmpty()
                                   group r by d.MonthOfYear into order
                                   select new
                                   {
                                       Interval = "Previous",
                                       Day = order.Key,
                                       TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                                   })
                        .Union(from d in currentMonth
                               from r in (from o in _orderRepository.Table
                                          where d.UtcTime <= o.CreatedOnUtc &&
                                          d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                          (storeId == 0 || storeId == o.StoreId) &&
                                          (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                          (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                          !o.Deleted
                                          select o).DefaultIfEmpty()
                               group r by d.MonthOfYear into order
                               select new
                               {
                                   Interval = "Current",
                                   Day = order.Key,
                                   TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                               })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                previousMonth.ForEach(p => result.Add(new
                {
                    Interval = "Previous",
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear) && d.Interval.Equals("Previous"))?.TotalSales ?? decimal.Zero
                }));

                currentMonth.ForEach(p => result.Add(new
                {
                    Interval = "Current",
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear) && d.Interval.Equals("Current"))?.TotalSales ?? decimal.Zero
                }));
            }

            return result;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="lastNYear">A value in indicating whether you want to load only total number of last n years records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardYearMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int lastNYear, bool priceWithTax)
        {
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);
            var fromYr = DateTime.Now.AddYears(-(lastNYear - 1)).Year;
            var years = _orderRepository.Table
                .Where(p => fromYr <= p.CreatedOnUtc.Year)
                .Select(p => p.CreatedOnUtc.Year)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var yearMonth = (from y in years
                             from r in Enumerable.Range(0, 12)
                             let now = new DateTime(y, 1, 1).AddMonths(r)
                             select new
                             {
                                 UtcTime = _dateTimeHelper.ConvertToUtcTime(now, timeZone),
                                 MonthOfYear = now.ToString("yyyy-MM")
                             }).OrderBy(y => y.UtcTime.Year).ThenBy(y => y.UtcTime.Month).ToList();

            var result = new List<object>();
            if (vendorId > 0)
            {
                var query = await (from d in yearMonth
                                   from r in (from oi in _orderItemRepository.Table
                                              join o in _orderRepository.Table on oi.OrderId equals o.Id
                                              join p in _productRepository.Table on oi.ProductId equals p.Id
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted && !p.Deleted &&
                                              (vendorId == 0 || p.VendorId == vendorId)
                                              select oi).DefaultIfEmpty()
                                   group r by d.MonthOfYear into orderItem
                                   let year = yearMonth.FirstOrDefault(y => y.MonthOfYear.Equals(orderItem.Key)).UtcTime.Year
                                   select new
                                   {
                                       Day = orderItem.Key,
                                       TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                                   })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                yearMonth.ForEach(p => result.Add(new
                {
                    Interval = p.UtcTime.Year,
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear))?.TotalSales ?? decimal.Zero
                }));
            }
            else
            {
                var query = await (from d in yearMonth
                                   from r in (from o in _orderRepository.Table
                                              where d.UtcTime <= o.CreatedOnUtc &&
                                              d.UtcTime.AddMonths(1) >= o.CreatedOnUtc &&
                                              (storeId == 0 || storeId == o.StoreId) &&
                                              (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                                              (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                                              !o.Deleted
                                              select o).DefaultIfEmpty()
                                   group r by d.MonthOfYear into order
                                   select new
                                   {
                                       Day = order.Key,
                                       TotalSales = priceWithTax ? order.Sum(order => order.OrderTotal) : order.Sum(order => (order.OrderTotal - order.OrderTax))
                                   })
                        .OrderBy(p => p.Day)
                        .ToListAsync();

                yearMonth.ForEach(p => result.Add(new
                {
                    Interval = p.UtcTime.Year,
                    Day = p.MonthOfYear,
                    TotalSales = query.FirstOrDefault(d => d.Day.Equals(p.MonthOfYear))?.TotalSales ?? decimal.Zero
                }));
            }

            return result;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardBestsellerProductsYearStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax)
        {
            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var currentYr = new DateTime(nowDt.Year, 1, 1);
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(currentYr, timeZone);

            var query = await (from oi in _orderItemRepository.Table
                               join o in _orderRepository.Table on oi.OrderId equals o.Id
                               join p in _productRepository.Table on oi.ProductId equals p.Id
                               where (startTimeUtc <= o.CreatedOnUtc) &&
                               (storeId == 0 || storeId == o.StoreId) &&
                               (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                               (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                               !o.Deleted && !p.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId)
                               group oi by p.Name into orderItem
                               select new
                               {
                                   Name = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderByDescending(p => p.TotalSales)
                        .Take(topNRecords)
                        .Select(p => p as object)
                        .ToListAsync();

            return query;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="lastNMonth">A value in indicating whether you want to load only total number of last n months records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardBestsellerProductsLastNMonthStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, int lastNMonth, bool priceWithTax)
        {
            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var nMonth = nowDt.AddMonths(-lastNMonth);
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(nMonth, timeZone);

            var query = await (from oi in _orderItemRepository.Table
                               join o in _orderRepository.Table on oi.OrderId equals o.Id
                               join p in _productRepository.Table on oi.ProductId equals p.Id
                               where (startTimeUtc <= o.CreatedOnUtc) &&
                               (storeId == 0 || storeId == o.StoreId) &&
                               (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                               (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                               !o.Deleted && !p.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId)
                               group oi by p.Name into orderItem
                               select new
                               {
                                   Name = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                         .OrderByDescending(p => p.TotalSales)
                         .Take(topNRecords)
                         .Select(p => p as object)
                         .ToListAsync();

            return query;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardBestsellerCategoriesStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax)
        {
            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var currentYr = new DateTime(nowDt.Year, 1, 1);
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(currentYr, timeZone);

            var query = await (from oi in _orderItemRepository.Table
                               join o in _orderRepository.Table on oi.OrderId equals o.Id
                               join p in _productRepository.Table on oi.ProductId equals p.Id
                               join pc in _productCategoryRepository.Table on oi.ProductId equals pc.ProductId
                               join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                               where (startTimeUtc <= o.CreatedOnUtc) &&
                               (storeId == 0 || storeId == o.StoreId) &&
                               (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                               (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                               !o.Deleted && !p.Deleted && !c.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId)
                               group oi by c.Name into orderItem
                               select new
                               {
                                   Name = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderByDescending(p => p.TotalSales)
                        .Take(topNRecords)
                        .Select(p => p as object)
                        .ToListAsync();

            return query;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardBestsellerManufacturersStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax)
        {
            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var currentYr = new DateTime(nowDt.Year, 1, 1);
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(currentYr, timeZone);

            var query = await (from oi in _orderItemRepository.Table
                               join o in _orderRepository.Table on oi.OrderId equals o.Id
                               join p in _productRepository.Table on oi.ProductId equals p.Id
                               join pm in _productManufacturerRepository.Table on oi.ProductId equals pm.ProductId
                               join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                               where (startTimeUtc <= o.CreatedOnUtc) &&
                               (storeId == 0 || storeId == o.StoreId) &&
                               (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                               (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                               !o.Deleted && !p.Deleted && !m.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId)
                               group oi by m.Name into orderItem
                               select new
                               {
                                   Name = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderByDescending(p => p.TotalSales)
                        .Take(topNRecords)
                        .Select(p => p as object)
                        .ToListAsync();

            return query;
        }

        /// <summary>
        /// Load Dashboard Chart Statistics
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all orders</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all orders</param>
        /// <param name="topNRecords">A value in indicating you want to load only total number of records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>List of data objects</returns>
        public async Task<List<object>> LoadDashboardBestsellerVendorsStatistics(int storeId, int vendorId, int orderStatusId, int paymentStatusId, int topNRecords, bool priceWithTax)
        {
            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            var currentYr = new DateTime(nowDt.Year, 1, 1);
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(currentYr, timeZone);

            var query = await (from oi in _orderItemRepository.Table
                               join o in _orderRepository.Table on oi.OrderId equals o.Id
                               join p in _productRepository.Table on oi.ProductId equals p.Id
                               join v in _vendorRepository.Table on p.VendorId equals v.Id
                               where (startTimeUtc <= o.CreatedOnUtc) &&
                               (storeId == 0 || storeId == o.StoreId) &&
                               (orderStatusId == 0 || orderStatusId == o.OrderStatusId) &&
                               (paymentStatusId == 0 || paymentStatusId == o.PaymentStatusId) &&
                               !o.Deleted && !p.Deleted && !v.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId)
                               group oi by v.Name into orderItem
                               select new
                               {
                                   Name = orderItem.Key,
                                   TotalSales = priceWithTax ? orderItem.Sum(orderItem => orderItem.PriceInclTax) : orderItem.Sum(orderItem => orderItem.PriceExclTax)
                               })
                        .OrderByDescending(p => p.TotalSales)
                        .Take(topNRecords)
                        .Select(p => p as object)
                        .ToListAsync();

            return query;
        }

        #endregion

        #region Products 

        #region Menu

        /// <summary>
        /// Load product menu product data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductMenuProductDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductMenuData(ProductReportEnum.ProductMenuProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product menu product aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductMenuProductAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchProductMenuAggrerator(ProductReportEnum.ProductMenuProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load product menu product attribute data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductMenuProductAttributeDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductMenuData(ProductReportEnum.ProductMenuProductAttribute, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product menu product attribute aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductMenuProductAttributeAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchProductMenuAggrerator(ProductReportEnum.ProductMenuProductAttribute, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load product menu category data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductMenuCategoryDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductMenuData(ProductReportEnum.ProductMenuCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product menu category aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductMenuCategoryAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchProductMenuAggrerator(ProductReportEnum.ProductMenuCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load product menu manufacturer data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductMenuManufacturerDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductMenuData(ProductReportEnum.ProductMenuManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product menu manufacturer aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductMenuManufacturerAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchProductMenuAggrerator(ProductReportEnum.ProductMenuManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load product menu vendor data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductMenuVendorDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductMenuData(ProductReportEnum.ProductMenuVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product menu vendor aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductMenuVendorAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchProductMenuAggrerator(ProductReportEnum.ProductMenuVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        #endregion

        #region Detail

        /// <summary>
        /// Load product detail product data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductDetailProductDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductDetailData(productId, ProductReportEnum.ProductDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get product detail product aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductDetailProductAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            return await SearchProductDetailAggrerator(productId, ProductReportEnum.ProductDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load product detail product attribute data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductDetailProductAttributeDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductDetailData(productId, ProductReportEnum.ProductDetailProductAttribute, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product detail product attribute aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductDetailProductAttributeAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchProductDetailAggrerator(productId, ProductReportEnum.ProductDetailProductAttribute, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load product detail customer data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductDetailCustomerDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductDetailData(productId, ProductReportEnum.ProductDetailCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product detail customer aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductDetailCustomerAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchProductDetailAggrerator(productId, ProductReportEnum.ProductDetailCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load product detail country data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductDetailCountryDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductDetailData(productId, ProductReportEnum.ProductDetailCountry, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get product detail country aggrerator
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadProductDetailCountryAggreratorAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchProductDetailAggrerator(productId, ProductReportEnum.ProductDetailCountry, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load product detail global data
        /// </summary>
        /// <param name="productId">product identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadProductDetailGlobalDataAsync(int productId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchProductDetailData(productId, ProductReportEnum.ProductDetailGlobal, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        #endregion

        #endregion

        #region Customer 

        #region Menu

        /// <summary>
        /// Load customer menu customer data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerMenuData(CustomerReportEnum.CustomerMenuCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer menu customer aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerMenuCustomerAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerMenuAggrerator(CustomerReportEnum.CustomerMenuCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load customer menu customer registered  data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerRegisteredDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerMenuData(CustomerReportEnum.CustomerMenuCustomerRegistered, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get customer menu customer registered aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerMenuCustomerRegisteredAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            return await SearchCustomerMenuAggrerator(CustomerReportEnum.CustomerMenuCustomerRegistered, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load customer menu customer gender data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerGenderDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerMenuData(CustomerReportEnum.CustomerMenuCustomerGender, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer menu customer gender aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerMenuCustomerGenderAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerMenuAggrerator(CustomerReportEnum.CustomerMenuCustomerGender, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load customer menu customer role data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCustomerRoleDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerMenuData(CustomerReportEnum.CustomerMenuCustomerRole, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer menu customer role aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerMenuCustomerRoleAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerMenuAggrerator(CustomerReportEnum.CustomerMenuCustomerRole, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        /// <summary>
        /// Load customer menu country data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerMenuCountryDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerMenuData(CustomerReportEnum.CustomerMenuCountry, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer menu country aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="customerRoleId">Customer Role identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerMenuCountryAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int customerRoleId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerMenuAggrerator(CustomerReportEnum.CustomerMenuCountry, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, customerRoleId, priceWithTax);
        }

        #endregion

        #region Detail

        /// <summary>
        /// Load customer detail customer data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailCustomerDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerDetailData(customerId, CustomerReportEnum.CustomerDetailCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get customer detail customer aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerDetailCustomerAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            return await SearchCustomerDetailAggrerator(customerId, CustomerReportEnum.CustomerDetailCustomer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load customer detail product data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailProductDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerDetailData(customerId, CustomerReportEnum.CustomerDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer detail product aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerDetailProductAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerDetailAggrerator(customerId, CustomerReportEnum.CustomerDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load customer detail category data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailCategoryDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerDetailData(customerId, CustomerReportEnum.CustomerDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer detail category aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerDetailCategoryAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerDetailAggrerator(customerId, CustomerReportEnum.CustomerDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load customer detail manufacturer data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailManufacturerDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerDetailData(customerId, CustomerReportEnum.CustomerDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer detail manufacturer aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerDetailManufacturerAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerDetailAggrerator(customerId, CustomerReportEnum.CustomerDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load customer detail vendor data
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCustomerDetailVendorDataAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchCustomerDetailData(customerId, CustomerReportEnum.CustomerDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get customer detail vendor aggrerator
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all order items</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCustomerDetailVendorAggreratorAsync(int customerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            return await SearchCustomerDetailAggrerator(customerId, CustomerReportEnum.CustomerDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId, priceWithTax);
        }

        #endregion

        #endregion

        #region Order

        /// <summary>
        /// Load Order menu order sales in time data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuOrderDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchOrderMenuData(OrderReportEnum.OrderMenuOrder, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get order menu order sales in time aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadOrderMenuOrderAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false)
        {
            return await SearchOrderMenuAggrerator(OrderReportEnum.OrderMenuOrder, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load Order menu order item sales in time data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuOrderItemDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchOrderMenuData(OrderReportEnum.OrderMenuOrderItem, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get order menu order item sales in time aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadOrderMenuOrderItemAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false)
        {
            return await SearchOrderMenuAggrerator(OrderReportEnum.OrderMenuOrderItem, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load Order menu order shipping method data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuShippingMethodDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchOrderMenuData(OrderReportEnum.OrderMenuShippingMethod, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get order menu order shipping method aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadOrderMenuShippingMethodAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false)
        {
            return await SearchOrderMenuAggrerator(OrderReportEnum.OrderMenuShippingMethod, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax);
        }

        /// <summary>
        /// Load Order menu order payment method data
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadOrderMenuPaymentMethodDataAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchOrderMenuData(OrderReportEnum.OrderMenuPaymentMethod, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get order menu order payment method aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="countryId">Country identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="shippingMethod">Shipping method. Leave empty to load all records.</param>
        /// <param name="paymentMethod">Payment method. Leave empty to load all records.</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadOrderMenuPaymentMethodAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int countryId = 0, int vendorId = 0, string shippingMethod = null, string paymentMethod = null, bool priceWithTax = false)
        {
            return await SearchOrderMenuAggrerator(OrderReportEnum.OrderMenuPaymentMethod, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, countryId, vendorId, shippingMethod, paymentMethod, priceWithTax);
        }

        #endregion

        #region Table

        /// <summary>
        /// Load table report
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<TableReport>> LoadTableReportAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchTableReport(createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId);

            query = query.OrderByDescending(p => p.CustomerId).ThenByDescending(p => p.YearMonth);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get table report aggrerator
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadTableReportAggreratorAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, int vendorId = 0)
        {
            return await SearchTableReportAggrerator(createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, vendorId);
        }

        #endregion

        #region Category

        /// <summary>
        /// Load category detail category data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailCategoryDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            var query = SearchCategoryDetailData(categoryIds, CategoryReportEnum.CategoryDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get category detail category aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCategoryDetailCategoryAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            return await SearchCategoryDetailAggrerator(categoryIds, CategoryReportEnum.CategoryDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load category detail product data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailProductDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            var query = SearchCategoryDetailData(categoryIds, CategoryReportEnum.CategoryDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get category detail product aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCategoryDetailProductAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            return await SearchCategoryDetailAggrerator(categoryIds, CategoryReportEnum.CategoryDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load category detail manufacturer data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailManufacturerDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            var query = SearchCategoryDetailData(categoryIds, CategoryReportEnum.CategoryDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get category detail manufacturer aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCategoryDetailManufacturerAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            return await SearchCategoryDetailAggrerator(categoryIds, CategoryReportEnum.CategoryDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load category detail vendor data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailVendorDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            var query = SearchCategoryDetailData(categoryIds, CategoryReportEnum.CategoryDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get category detail vendor aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCategoryDetailVendorAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            return await SearchCategoryDetailAggrerator(categoryIds, CategoryReportEnum.CategoryDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);
        }

        /// <summary>
        /// Load category detail categories share data
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadCategoryDetailCategoriesShareDataAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            var query = SearchCategoryDetailData(categoryIds, CategoryReportEnum.CategoryDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get category detail categories share aggrerator
        /// </summary>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadCategoryDetailCategoriesShareAggreratorAsync(int categoryId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int manufacturerId = 0, int vendorId = 0, bool priceWithTax = false)
        {
            var categoryIds = new List<int> { categoryId };
            //include subcategories
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: categoryId, showHidden: true));

            return await SearchCategoryDetailAggrerator(categoryIds, CategoryReportEnum.CategoryDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, manufacturerId, vendorId, priceWithTax);
        }

        #endregion

        #region Manufacturer

        /// <summary>
        /// Load manufacturer detail manufacturer data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailManufacturerDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchManufacturerDetailData(manufacturerId, ManufacturerReportEnum.ManufacturerDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get manufacturer detail manufacturer aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadManufacturerDetailManufacturerAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            return await SearchManufacturerDetailAggrerator(manufacturerId, ManufacturerReportEnum.ManufacturerDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load manufacturer detail product data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailProductDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchManufacturerDetailData(manufacturerId, ManufacturerReportEnum.ManufacturerDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get manufacturer detail product aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadManufacturerDetailProductAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false)
        {
            return await SearchManufacturerDetailAggrerator(manufacturerId, ManufacturerReportEnum.ManufacturerDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);
        }

        /// <summary>
        /// Load manufacturer detail category data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailCategoryDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchManufacturerDetailData(manufacturerId, ManufacturerReportEnum.ManufacturerDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get manufacturer detail category aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadManufacturerDetailCategoryAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false)
        {
            return await SearchManufacturerDetailAggrerator(manufacturerId, ManufacturerReportEnum.ManufacturerDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);
        }

        /// <summary>
        /// Load manufacturer detail manufacturers share data
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadManufacturerDetailManufacturersShareDataAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchManufacturerDetailData(manufacturerId, ManufacturerReportEnum.ManufacturerDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get manufacturer detail manufacturers share aggrerator
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadManufacturerDetailManufacturersShareAggreratorAsync(int manufacturerId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, bool priceWithTax = false)
        {
            return await SearchManufacturerDetailAggrerator(manufacturerId, ManufacturerReportEnum.ManufacturerDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, priceWithTax);
        }

        #endregion

        #region Vendor

        /// <summary>
        /// Load vendor detail vendor data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailVendorDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchVendorDetailData(vendorId, VendorReportEnum.VendorDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax, groupBy);

            query = query.OrderByDescending(p => p.Title);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            //Prepare group by title
            PrepareGroupByTitle(result, groupBy);

            return result;
        }

        /// <summary>
        /// Get vendor detail vendor aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="groupBy">Group identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadVendorDetailVendorAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, ReportGroupByOptions groupBy = ReportGroupByOptions.Month, bool priceWithTax = false)
        {
            return await SearchVendorDetailAggrerator(vendorId, VendorReportEnum.VendorDetailVendor, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax, groupBy);
        }

        /// <summary>
        /// Load vendor detail product data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailProductDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchVendorDetailData(vendorId, VendorReportEnum.VendorDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get vendor detail product aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadVendorDetailProductAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false)
        {
            return await SearchVendorDetailAggrerator(vendorId, VendorReportEnum.VendorDetailProduct, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);
        }

        /// <summary>
        /// Load vendor detail category data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailCategoryDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchVendorDetailData(vendorId, VendorReportEnum.VendorDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get vendor detail category aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadVendorDetailCategoryAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false)
        {
            return await SearchVendorDetailAggrerator(vendorId, VendorReportEnum.VendorDetailCategory, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);
        }

        /// <summary>
        /// Load vendor detail manufacturer data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailManufacturerDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchVendorDetailData(vendorId, VendorReportEnum.VendorDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get vendor detail manufacturer aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadVendorDetailManufacturerAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false)
        {
            return await SearchVendorDetailAggrerator(vendorId, VendorReportEnum.VendorDetailManufacturer, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);
        }

        /// <summary>
        /// Load vendor detail vendors share data
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of data objects</returns>
        public async Task<IPagedList<ChartsAndTablesData>> LoadVendorDetailVendorsShareDataAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SearchVendorDetailData(vendorId, VendorReportEnum.VendorDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);

            query = query.OrderByDescending(p => p.TotalAmount);

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }

        /// <summary>
        /// Get vendor detail vendors share aggrerator
        /// </summary>
        /// <param name="vendorId">Vendor identifier; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier; null to load all order items</param>
        /// <param name="orderStatusId">Order status identifiers; null to load all records</param>
        /// <param name="paymentStatusId">Payment status identifiers; null to load all records</param>
        /// <param name="categoryId">Category identifier; null to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; null to load all records</param>
        /// <param name="priceWithTax">A value in indicating you want to load price included tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<AggreratorLine> LoadVendorDetailVendorsShareAggreratorAsync(int vendorId, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int storeId = 0, int orderStatusId = 0, int paymentStatusId = 0, int categoryId = 0, int manufacturerId = 0, bool priceWithTax = false)
        {
            return await SearchVendorDetailAggrerator(vendorId, VendorReportEnum.VendorDetailShare, createdFromUtc, createdToUtc, storeId, orderStatusId, paymentStatusId, categoryId, manufacturerId, priceWithTax);
        }

        #endregion

        #endregion
    }
}
