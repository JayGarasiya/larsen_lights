using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles;

/// <summary>
/// Represents a discount requirement rule that checks whether a customer has a restricted role.
/// </summary>
public class MustNotCustomerRoleDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
{
    #region Fields
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly IWebHelper _webHelper;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    #endregion

    #region Ctor
    public MustNotCustomerRoleDiscountRequirementRule(
        IDiscountService discountService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        INopUrlHelper nopUrlHelper)
    {
        _customerService = customerService;
        _discountService = discountService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _nopUrlHelper = nopUrlHelper;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Checks if the discount requirement is valid based on customer roles.
    /// </summary>
    /// <param name="request">The request object containing customer and discount information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, 
    /// with the result indicating if the requirement is valid or not.
    /// </returns>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        // Ensure the request is not null
        ArgumentNullException.ThrowIfNull(request);

        // Assume discount is invalid by default
        var result = new DiscountRequirementValidationResult();

        // If there is no customer, return invalid result
        if (request.Customer == null)
            return result;

        // Retrieve the restricted role ID(s) from the plugin settings
        var restrictedRoleId = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));
        var restrictedRoleIds = restrictedRoleId.Split(',').Select(int.Parse).ToList();

        // If no restricted role is found, return invalid
        if (!restrictedRoleIds.Any())
            return result;

        // Get the current customer roles
        var currentCustomerRoles = await _customerService.GetCustomerRolesAsync(request.Customer);

        // Check if the customer belongs to any of the restricted roles
        var restricted = currentCustomerRoles.Any(role => restrictedRoleIds.Contains(role.Id) && role.Active);

        // Get the store scope configuration
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        // Get dealer role IDs from settings and check if the customer belongs to any of them
        var dealerRoleIds = await _settingService.GetSettingByKeyAsync("productextensionsettings.dealerroleids", new List<int>(), storeScope == 0 ? 0 : request.Store.Id);
        var dealer = currentCustomerRoles.Any(role => dealerRoleIds.Any(dr => dr == role.Id));

        // If customer is not a dealer, the discount is valid if the customer is not restricted
        if (!dealer)
        {
            result.IsValid = !restricted;
        }
        else
        {
            // For dealers, check if the "DealerPrice" attribute exists and its value
            var currentPriceId = await _genericAttributeService.GetAttributeAsync(request.Customer, "DealerPrice", request.Store.Id, 1);

            // If the price ID is 2, invert the discount condition (apply discount if restricted and dealer)
            if (currentPriceId == 2)            //[1= Dealer price, 2= List price]
                result.IsValid = restricted && dealer;
            else
                result.IsValid = !restricted;
        }

        return result;
    }

    /// <summary>
    /// Returns the URL for configuring the discount requirement rule.
    /// </summary>
    /// <param name="discountId">The ID of the discount.</param>
    /// <param name="discountRequirementId">The ID of the discount requirement.</param>
    /// <returns>The configuration URL.</returns>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        // Generate and return the URL to configure this discount rule
        return _nopUrlHelper.RouteUrl(DiscountRequirementDefaults.ConfigurationRouteName,
            new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
    }

    /// <summary>
    /// Installs the plugin, including adding necessary locale resources.
    /// </summary>
    /// <returns>A task representing the asynchronous installation operation.</returns>
    public override async Task InstallAsync()
    {
        // Add locale resources for this plugin (text displayed in UI)
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRole"] = "Required customer role",
            ["Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRole.Hint"] = "Discount will not be applied if customer is in the selected customer role.",
            ["Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRole.Select"] = "Select customer role",
            ["Plugins.DiscountRules.MustNotCustomerRoles.Fields.CustomerRoleId.Required"] = "Customer role is required",
            ["Plugins.DiscountRules.MustNotCustomerRoles.Fields.DiscountId.Required"] = "Discount is required"
        });

        // Complete installation process from the base class
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstalls the plugin, including removing associated discount requirements and locale resources.
    /// </summary>
    /// <returns>A task representing the asynchronous uninstallation operation.</returns>
    public override async Task UninstallAsync()
    {
        // Find and delete discount requirements related to this plugin
        var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME);

        foreach (var discountRequirement in discountRequirements)
        {
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
        }

        // Delete locale resources for this plugin
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.MustNotCustomerRoles");

        // Complete uninstallation process from the base class
        await base.UninstallAsync();
    }

    #endregion
}
