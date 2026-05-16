using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.SpendAmountOver.Models;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.SpendAmountOver.Controllers;

public class DiscountRulesSpentAmountOverController : BaseAdminController
{
    #region Fields
    protected readonly IDiscountService _discountService;
    protected readonly ISettingService _settingService;
    protected readonly IVendorService _vendorService;
    #endregion

    #region Ctor
    public DiscountRulesSpentAmountOverController(IDiscountService discountService,
        ISettingService settingService,
        IVendorService vendorService)
    {
        _discountService = discountService;
        _settingService = settingService;
        _vendorService = vendorService;
    }
    #endregion

    #region Utilities
    private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
    {
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }
    #endregion

    #region Methods

    #region Configure
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        // Load the discount by ID
        var discount = await _discountService.GetDiscountByIdAsync(discountId)
                       ?? throw new ArgumentException("Discount could not be loaded");

        // Check whether the discount requirement exists
        if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
            return Content("Failed to load requirement.");

        // Fetch setting values for spend amount and selected vendors
        var spendAmountRequirement = await _settingService.GetSettingByKeyAsync<decimal>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirementId ?? 0));
        var vendorsRequirement = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.VENDORS_SETTINGS_KEY, discountRequirementId ?? 0));

        // Prepare the model to display on the view
        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId,
            SpendAmount = spendAmountRequirement,
        };

        // Parse the vendors list if available
        if (!string.IsNullOrWhiteSpace(vendorsRequirement))
            model.SelectedVendorIds = vendorsRequirement.Split(',').Select(int.Parse).ToList();

        // Load available vendors for selection
        model.AvailableVendors = (await _vendorService.GetAllVendorsAsync(showHidden: true)).Select(v => new SelectListItem
        {
            Text = v.Name,
            Value = v.Id.ToString(),
            Selected = model.SelectedVendorIds.Contains(v.Id)
        }).ToList();

        // Add a prefix to the HTML fields for correct model binding
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);

        // Return the view with the populated model
        return View("~/Plugins/DiscountRules.SpendAmountOver/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (ModelState.IsValid)
        {
            // Load the discount object by ID
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            // Load or create the discount requirement
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SYSTEM_NAME
                };

                // Insert the new discount requirement
                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
            }

            // Save settings for the discount requirement
            await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id), model.SpendAmount);
            await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.VENDORS_SETTINGS_KEY, discountRequirement.Id), string.Join(",", model.SelectedVendorIds));

            // Return the new requirement ID
            return Ok(new { NewRequirementId = discountRequirement.Id });
        }

        // If model state is invalid, return errors
        return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
    }
    #endregion

    #endregion

}
