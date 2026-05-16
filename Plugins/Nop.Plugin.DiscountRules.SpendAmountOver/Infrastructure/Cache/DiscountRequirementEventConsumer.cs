using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace Nop.Plugin.DiscountRules.SpendAmountOver.Infrastructure.Cache;

/// <summary>
/// Event consumer that handles the deletion of discount requirements.
/// </summary>
public partial class DiscountRequirementEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
{
    #region Fields
    private readonly ISettingService _settingService;
    #endregion

    #region Ctor
    public DiscountRequirementEventConsumer(ISettingService settingService)
    {
        _settingService = settingService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Handles the event when a discount requirement is deleted.
    /// Deletes the associated restricted customer role identifier from settings.
    /// </summary>
    /// <param name="eventMessage">The event message containing the deleted discount requirement.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
    {
        var discountRequirement = eventMessage?.Entity;
        if (discountRequirement == null)
            return;

        // Try to fetch the associated setting and delete it if it exists
        var setting = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id));
        if (setting != null)
            await _settingService.DeleteSettingAsync(setting);
    }
    #endregion
}

