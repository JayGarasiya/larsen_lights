using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Models;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Controllers
{
    public class OverrideReportController : ReportController
    {

        #region Fields
        protected readonly ILocalizationService _localizationService;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IProductService _productService;
        protected readonly IProductAttributeFormatter _productAttributeFormatter;
        protected readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public OverrideReportController(
            IPermissionService permissionService,
            IReportModelFactory reportModelFactory,
            ILocalizationService localizationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IWorkContext workContext,
            IStoreContext storeContext,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IProductService productService,
            IProductAttributeFormatter productAttributeFormatter,
            ICategoryService categoryService) : base (permissionService,
                reportModelFactory)
        {
            _localizationService = localizationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _workContext = workContext;
            _storeContext = storeContext;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _productService = productService;
            _productAttributeFormatter = productAttributeFormatter;
            _categoryService = categoryService;
        }

        #endregion

        #region Utilities

        #region LowStock

        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; pass null to load all records</param>
        /// <param name="loadPublishedOnly">Whether to load published products only; pass null to load all products, pass true to load only published products, pass false to load only unpublished products</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public virtual async Task<IPagedList<Product>> GetLowStockProductsAsync(int? vendorId = null, IList<int> categoryIds = null,
            IList<int> manufacturerIds = null, int warehouseId = 0, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _productRepository.Table;

            //filter by products with tracking inventory
            query = query.Where(product => product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock);

            //filter by products with stock quantity less than the minimum
            query = query.Where(product =>
                (product.UseMultipleWarehouses ? _productWarehouseInventoryRepository.Table.Where(pwi => pwi.ProductId == product.Id).Sum(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                    : product.StockQuantity) <= product.MinStockQuantity);

            //ignore deleted products and filter by warehouse
            query = query.Where(product => !product.Deleted && (
                        warehouseId == 0 ||
                        (
                            !product.UseMultipleWarehouses ? product.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == product.Id)
                        )
                    ));

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

                    query =
                        from p in query
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    query =
                        from p in query
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            //ignore grouped products
            query = query.Where(product => product.ProductTypeId != (int)ProductType.GroupedProduct);

            //filter by vendor
            if (vendorId.HasValue && vendorId.Value > 0)
                query = query.Where(product => product.VendorId == vendorId.Value);

            //whether to load published products only
            if (loadPublishedOnly.HasValue)
                query = query.Where(product => product.Published == loadPublishedOnly.Value);

            query = query.OrderBy(product => product.MinStockQuantity).ThenBy(product => product.DisplayOrder).ThenBy(product => product.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Get low stock product combinations
        /// </summary>
        /// <param name="vendorId">Vendor identifier; pass null to load all records</param>
        /// <param name="loadPublishedOnly">Whether to load combinations of published products only; pass null to load all products, pass true to load only published products, pass false to load only unpublished products</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product combinations
        /// </returns>
        public virtual async Task<IPagedList<ProductAttributeCombination>> GetLowStockProductCombinationsAsync(int? vendorId = null, IList<int> categoryIds = null,
            IList<int> manufacturerIds = null, int warehouseId = 0, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var combinations = from pac in _productAttributeCombinationRepository.Table
                               join p in _productRepository.Table on pac.ProductId equals p.Id
                               where
                                   //filter by combinations with stock quantity less than the minimum
                                   pac.StockQuantity <= pac.MinStockQuantity &&
                                   //filter by products with tracking inventory by attributes
                                   p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                                   //ignore deleted products and filter by warehouse
                                   !p.Deleted && (
                                        warehouseId == 0 ||
                                        (
                                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                                        )
                                    ) &&
                                   //ignore grouped products
                                   p.ProductTypeId != (int)ProductType.GroupedProduct &&
                                   //filter by vendor
                                   ((vendorId ?? 0) == 0 || p.VendorId == vendorId) &&
                                   //whether to load published products only
                                   (loadPublishedOnly == null || p.Published == loadPublishedOnly)
                               orderby pac.ProductId, pac.Id
                               select pac;

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

                    combinations =
                        from p in combinations
                        join pc in productCategoryQuery on p.ProductId equals pc.ProductId
                        orderby pc.DisplayOrder
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    combinations =
                        from p in combinations
                        join pm in productManufacturerQuery on p.ProductId equals pm.ProductId
                        orderby pm.DisplayOrder
                        select p;
                }
            }

            return await combinations.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Prepare low stock product search model
        /// </summary>
        /// <param name="searchModel">Low stock product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the low stock product search model
        /// </returns>
        public virtual async Task<LowStockProductSearchModel> PrepareLowStockProductSearchModelAsync(OverrideLowStockProductSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Reports.LowStock.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Reports.LowStock.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Reports.LowStock.SearchPublished.UnpublishedOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged low stock product list model
        /// </summary>
        /// <param name="searchModel">Low stock product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the low stock product list model
        /// </returns>
        public virtual async Task<LowStockProductListModel> PrepareLowStockProductListModelAsync(OverrideLowStockProductSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //get parameters to filter comments
            var publishedOnly = searchModel.SearchPublishedId == 0 ? null : searchModel.SearchPublishedId == 1 ? true : (bool?)false;
            var vendor = await _workContext.GetCurrentVendorAsync();
            var vendorId = vendor?.Id ?? 0;

            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get low stock product and product combinations
            var products = await GetLowStockProductsAsync(vendorId: vendorId, categoryIds: categoryIds, manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                warehouseId: searchModel.SearchWarehouseId, loadPublishedOnly: publishedOnly);
            var combinations = await GetLowStockProductCombinationsAsync(vendorId: vendorId, categoryIds: categoryIds, manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                warehouseId: searchModel.SearchWarehouseId, loadPublishedOnly: publishedOnly);

            //prepare low stock product models
            var lowStockProductModels = new List<LowStockProductModel>();
            lowStockProductModels.AddRange(await products.SelectAwait(async product => new LowStockProductModel
            {
                Id = product.Id,
                Name = product.Name,

                ManageInventoryMethod = await _localizationService.GetLocalizedEnumAsync(product.ManageInventoryMethod),
                StockQuantity = await _productService.GetTotalStockQuantityAsync(product),
                Published = product.Published
            }).ToListAsync());

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            lowStockProductModels.AddRange(await combinations.SelectAwait(async combination =>
            {
                var product = await _productService.GetProductByIdAsync(combination.ProductId);

                return new LowStockProductModel
                {
                    Id = combination.ProductId,
                    Name = product.Name,

                    Attributes = await _productAttributeFormatter
                        .FormatAttributesAsync(product, combination.AttributesXml, currentCustomer, currentStore, "<br />", true, true, true, false),
                    ManageInventoryMethod = await _localizationService.GetLocalizedEnumAsync(product.ManageInventoryMethod),

                    StockQuantity = combination.StockQuantity,
                    Published = product.Published
                };
            }).ToListAsync());

            var pagesList = lowStockProductModels.ToPagedList(searchModel);

            //prepare list model
            var model = new LowStockProductListModel().PrepareToGrid(searchModel, pagesList, () => pagesList);

            return model;
        }

        #endregion

        #endregion

        #region Methods

        #region Low stock
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public override async Task<IActionResult> LowStock()
        {
            //prepare model
            var model = await PrepareLowStockProductSearchModelAsync(new OverrideLowStockProductSearchModel());

            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> LowStockProductList(OverrideLowStockProductSearchModel searchModel)
        {
            //prepare model
            var model = await PrepareLowStockProductListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #endregion
    }
}
