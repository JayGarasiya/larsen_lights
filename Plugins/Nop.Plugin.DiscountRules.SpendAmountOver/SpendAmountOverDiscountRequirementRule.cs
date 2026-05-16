using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.SpendAmountOver;

public class SpendAmountOverDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
{
    #region Fields
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly ICategoryService _categoryService;
    protected readonly IProductService _productService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly INopUrlHelper _nopUrlHelper;
    #endregion

    #region Ctor
    public SpendAmountOverDiscountRequirementRule(
        IDiscountService discountService,
        ILocalizationService localizationService,
        IShoppingCartService shoppingCartService,
        ICategoryService categoryService,
        IProductService productService,
        ISettingService settingService,
        IWebHelper webHelper,
        INopUrlHelper nopUrlHelper)
    {
        _discountService = discountService;
        _localizationService = localizationService;
        _shoppingCartService = shoppingCartService;
        _categoryService = categoryService;
        _productService = productService;
        _settingService = settingService;
        _webHelper = webHelper;
        _nopUrlHelper = nopUrlHelper;
    }
    #endregion

    #region Methods

    #region Check Requirement
    /// <summary>
    /// Checks whether the discount requirement is satisfied based on the cart's total spend amount.
    /// </summary>
    /// <param name="request">Object that contains all information required to check the requirement (e.g., Current customer, discount, etc)</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// DiscountRequirementValidationResult containing the result of the validation
    /// </returns>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        // Ensure the request is not null
        ArgumentNullException.ThrowIfNull(request);

        // Invalid by default
        var result = new DiscountRequirementValidationResult();

        // Get the required spend amount for the discount rule
        var spendAmountRequirement = await _settingService.GetSettingByKeyAsync<decimal>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));
        if (spendAmountRequirement == decimal.Zero)
        {
            // If no spend amount requirement, automatically valid
            result.IsValid = true;
            return result;
        }

        // If no customer, return invalid result
        if (request.Customer == null)
            return result;

        // Fetch the customer's shopping cart
        var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(request.Customer, ShoppingCartType.ShoppingCart, request.Store.Id);
        if (shoppingCart == null)
            return result;

        // Get the vendors that are excluded from the discount
        var vendorsRequirement = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.VENDORS_SETTINGS_KEY, request.DiscountRequirementId));
        var restrictedVendorIds = !string.IsNullOrWhiteSpace(vendorsRequirement) ? vendorsRequirement.Split(',').Select(int.Parse).ToList() : new List<int>();

        // Load the discount and check its type to determine if it's applicable
        var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(request.DiscountRequirementId);
        var discount = await _discountService.GetDiscountByIdAsync(discountRequirement.DiscountId);
        var includedDiscount = discount.DiscountType switch
        {
            DiscountType.AssignedToOrderTotal => true,
            DiscountType.AssignedToOrderSubTotal => true,
            DiscountType.AssignedToShipping => true,
            _ => false
        };

        // Retrieve any category restrictions from another discount requirement (if applicable)
        var restrictedCategoryIds = string.Empty;
        var hasOneCategoryRequirements = (await _discountService.GetAllDiscountRequirementsAsync(discountRequirement.DiscountId))
            .FirstOrDefault(d => d.Id != request.DiscountRequirementId && d.DiscountRequirementRuleSystemName == "DiscountRequirement.HasOneCategory");

        if (hasOneCategoryRequirements != null)
            restrictedCategoryIds = await _settingService.GetSettingByKeyAsync<string>(string.Format("DiscountRequirement.RestrictedCategoryIds-{0}", hasOneCategoryRequirements.Id));

        // Calculate the total spend amount
        decimal spendAmount = decimal.Zero;
        if (!string.IsNullOrEmpty(restrictedCategoryIds))
        {
            // If category restrictions are present, only include items from those categories
            var restrictedCategories = restrictedCategoryIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x.Trim()))
                .ToList();
            if (!restrictedCategories.Any())
                return result;

            foreach (var sci in shoppingCart)
            {
                var productCategory = await _categoryService.GetProductCategoriesByProductIdAsync(sci.ProductId);
                if (productCategory.Any(c => restrictedCategories.Contains(c.CategoryId)))
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);
                    if (restrictedVendorIds.Any(rv => rv == product.VendorId))
                        continue;

                    var (subtotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, includedDiscount);
                    spendAmount += subtotal;
                }
            }
        }
        else
        {
            // No category restrictions, calculate spend for all cart items
            if (request.Customer.HasShoppingCartItems)
            {
                foreach (var sci in shoppingCart)
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);
                    if (restrictedVendorIds.Any(rv => rv == product.VendorId))
                        continue;

                    var (subtotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, includedDiscount);
                    spendAmount += subtotal;
                }
            }
        }

        // Validate if the spend amount is greater than or equal to the required amount
        if (spendAmount > spendAmountRequirement)
            result.IsValid = true;
        else
            result.UserError = await _localizationService.GetResourceAsync("Plugins.DiscountRules.SpendAmountOver.NotEnough");

        return result;
    }
    #endregion

    #region Configuration URL
    /// <summary>
    /// Retrieves the URL for configuring the discount rule.
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
    /// <returns>Configuration URL</returns>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        return _nopUrlHelper.RouteUrl(DiscountRequirementDefaults.ConfigurationRouteName,
            new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
    }
    #endregion

    #region In/Uninstall

    /// <summary>
    /// Installs the plugin
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        // Add or update locale resources for the plugin
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.DiscountRules.SpendAmountOver.Fields.Amount"] = "Required spend amount",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.Amount.Hint"] = "Discount will be applied if customer spends over x.xx amount.",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.SpendAmount.Required"] = "Spend amount should be greater than zero (0)",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.Vendors"] = "Exclude vendor's products",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.Vendors.Hint"] = "Choose vendors to exclude their products from discount.",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.Vendors.NoVendors"] = "No vendors available. Create at least one vendor before mapping.",
            ["Plugins.DiscountRules.SpendAmountOver.Fields.DiscountId.Required"] = "Discount is required",
            ["Plugins.DiscountRules.SpendAmountOver.NotEnough"] = "Sorry, this offer requires more money to be spent (on the current cart)"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        // Delete discount requirements related to this plugin
        var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(dr => dr.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME ||
                         dr.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.VENDORS_SETTINGS_KEY);
        foreach (var discountRequirement in discountRequirements)
        {
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
        }

        // Delete plugin locale resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.SpendAmountOver");

        await base.UninstallAsync();
    }
    #endregion

    #endregion
}
