using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.MustNotCustomerRoles.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles.Controllers;

public class DiscountRulesMustNotCustomerRolesController : BaseAdminController
{
    #region Fields
    protected readonly ICustomerService _customerService;
    protected readonly IDiscountService _discountService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    #endregion

    #region Ctor
    public DiscountRulesMustNotCustomerRolesController(ICustomerService customerService,
        IDiscountService discountService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _customerService = customerService;
        _discountService = discountService;
        _permissionService = permissionService;
        _settingService = settingService;
    }
    #endregion

    #region Utilities
    private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
    {
        // Collect and return all errors from the model state
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }
    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        // Load the discount by its ID and handle failure to load
        var discount = await _discountService.GetDiscountByIdAsync(discountId)
                       ?? throw new ArgumentException("Discount could not be loaded");

        // Check if the discount requirement exists, if provided
        if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
            return Content("Failed to load requirement.");

        // Try to fetch previously saved restricted customer role identifier (if exists)
        var restrictedRoleId = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirementId ?? 0));

        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId
        };

        // Populate the selected customer role IDs if there are saved settings
        if (!string.IsNullOrWhiteSpace(restrictedRoleId))
            model.SelectedCustomerRoleIds = restrictedRoleId.Split(',').Select(int.Parse).ToList();

        // Fetch all available customer roles and create a list of SelectListItem for the UI
        model.AvailableCustomerRoles = (await _customerService.GetAllCustomerRolesAsync(true)).Select(role => new SelectListItem
        {
            Text = role.Name,
            Value = role.Id.ToString(),
            Selected = model.SelectedCustomerRoleIds.Contains(role.Id)  // Mark as selected if the role is part of the selected roles
        }).ToList();

        // Set the HTML field prefix for form elements (helpful for validation and correct naming)
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);

        // Return the view for configuring the discount rule
        return View("~/Plugins/DiscountRules.MustNotCustomerRoles/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        // Validate the model state before processing
        if (ModelState.IsValid)
        {
            // Load the discount by its ID
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            // Get the discount requirement by ID
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

            // If the discount requirement does not exist, create a new one
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SYSTEM_NAME
                };

                await _discountService.InsertDiscountRequirementAsync(discountRequirement); // Save the new requirement
            }

            // Save the restricted customer role identifiers (selected roles) to settings
            await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id), string.Join(",", model.SelectedCustomerRoleIds));

            return Ok(new { NewRequirementId = discountRequirement.Id }); // Return the ID of the new requirement
        }

        // If the model state is invalid, return the errors
        return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
    }
    #endregion

}