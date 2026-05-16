using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.BulkEdit.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.BulkEdit.Areas.Admin.Controllers;

public class BulkEditProductController : BaseAdminController
{
    #region Fields
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly IProductService _productService;
    protected readonly AdminAreaSettings _adminAreaSettings;
    protected readonly IWorkContext _workContext;
    protected readonly ICurrencyService _currencyService;
    protected readonly CurrencySettings _currencySettings;
    protected readonly TaxSettings _taxSettings;
    #endregion

    #region Ctor
    public BulkEditProductController(
        IProductService productService,
        IProductModelFactory productModelFactory,
        AdminAreaSettings adminAreaSettings,
        IWorkContext workContext,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        TaxSettings taxSettings)
    {
        _productService = productService;
        _productModelFactory = productModelFactory;
        _adminAreaSettings = adminAreaSettings;
        _workContext = workContext;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _taxSettings = taxSettings;
    }
    #endregion

    #region Utilities
    protected async Task<List<BulkEditData>> ParseBulkEditDataAsync()
    {
        var rez = new Dictionary<int, BulkEditData>();
        var currentVendor = await _workContext.GetCurrentVendorAsync();

        // Loop through each form field and extract product-related information
        foreach (var item in Request.Form)
        {
            // If the form key matches "product-select-", extract productId and mark it as selected
            if (getData(item, "product-select-", out var productId))
                setData(productId, data =>
                {
                    data.IsSelected = true; // Mark product as selected
                });

            // Extract name, sku, cost, price, old-price, quantity, and published status from the form data
            if (getData(item, "name-", out productId))
                setData(productId, data =>
                {
                    data.Name = item.Value;
                });

            if (getData(item, "sku-", out productId))
                setData(productId, data =>
                {
                    data.Sku = item.Value;
                });

            if (getData(item, "cost-", out productId))
                setData(productId, data =>
                {
                    data.ProductCost = decimal.Parse(item.Value);
                });

            if (getData(item, "price-", out productId))
                setData(productId, data =>
                {
                    data.Price = decimal.Parse(item.Value);
                });

            if (getData(item, "old-price-", out productId))
                setData(productId, data =>
                {
                    data.OldPrice = decimal.Parse(item.Value);
                });

            if (getData(item, "quantity-", out productId))
                setData(productId, data =>
                {
                    data.Quantity = int.Parse(item.Value);
                });

            if (getData(item, "published-", out productId))
                setData(productId, data =>
                {
                    data.IsPublished = true;
                });
        }

        var productIds = rez.Select(p => p.Key).ToArray();

        var products = await _productService.GetProductsByIdsAsync(productIds); // Retrieve product details by IDs

        // Assign products to their corresponding bulk edit data
        foreach (var product in products)
            rez[product.Id].Product = product;

        return rez.Values.ToList();

        // Helper method to extract data based on the form key
        bool getData(KeyValuePair<string, StringValues> item, string selector, out int productId)
        {
            var key = item.Key;
            productId = 0;

            if (!key.StartsWith(selector))
                return false;

            productId = int.Parse(key.Replace(selector, string.Empty));

            return true;
        }

        // Helper method to set values on the parsed data
        void setData(int productId, Action<BulkEditData> action)
        {
            if (!rez.ContainsKey(productId))
                rez.Add(productId, new BulkEditData(_taxSettings.DefaultTaxCategoryId, currentVendor?.Id ?? 0));

            action(rez[productId]);
        }
    }
    #endregion

    #region Methods
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public async Task<IActionResult> BulkEdit()
    {
        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());
        model.Length = _adminAreaSettings.ProductsBulkEditGridPageSize;

        return View("~/Plugins/Misc.BulkEdit/Areas/Admin/Views/Product/BulkEdit.cshtml", model); 
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public async Task<IActionResult> BulkEditProducts(ProductSearchModel searchModel)
    {
        // Prepare the product list model for the given search criteria
        var model = await _productModelFactory.PrepareProductListModelAsync(searchModel);

        // Render HTML for the partial view of plugin 
        var html = await RenderPartialViewToStringAsync("~/Plugins/Misc.BulkEdit/Areas/Admin/Views/Product/_BulkEdit.Products.cshtml",
                                                        model.Data.ToList());

        // Return HTML and model data as JSON
        return Json(new Dictionary<string, object> { { "Html", html }, { "Products", model } });
    }

    [HttpPost, ActionName("BulkEdit"), ParameterBasedOnFormName("bulk-edit-save-selected", "selected")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> BulkEditSave(ProductSearchModel searchModel, bool selected)
    {
        var data = await ParseBulkEditDataAsync(); // Parse the bulk edit data

        // Filter products that need updating or creation based on selected flag
        var productsToUpdate = data.Where(d => d.NeedToUpdate(selected)).ToList();
        await _productService.UpdateProductsAsync(productsToUpdate.Select(d => d.UpdateProduct(selected)).ToList());

        var productsToInsert = data.Where(d => d.NeedToCreate(selected)).ToList();
        await _productService.InsertProductsAsync(productsToInsert.Select(d => d.CreateProduct(selected)).ToList());

        // Prepare the product search model for display after bulk edit
        var model = await _productModelFactory.PrepareProductSearchModelAsync(searchModel);
        model.Length = _adminAreaSettings.ProductsBulkEditGridPageSize;

        // Return the view with the updated model
        return View("~/Plugins/Misc.BulkEdit/Areas/Admin/Views/Product/BulkEdit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public async Task<IActionResult> BulkEditNewProduct(int id)
    {
        var primaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

        //prepare model
        var model = new List<ProductModel> { new()
        {
            Id = id,
            PrimaryStoreCurrencyCode = primaryStoreCurrencyCode,
            Published = true
        } };

        var html = await RenderPartialViewToStringAsync("~/Plugins/Misc.BulkEdit/Areas/Admin/Views/Product/_BulkEdit.Products.cshtml", model);

        return Json(html);
    }
    #endregion
}
