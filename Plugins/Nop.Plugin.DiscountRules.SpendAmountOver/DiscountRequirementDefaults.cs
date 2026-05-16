namespace Nop.Plugin.DiscountRules.SpendAmountOver;

/// <summary>
/// Represents constants for the discount requirement rule
/// </summary>
public static class DiscountRequirementDefaults
{
    /// <summary>
    /// The system name of the discount requirement rule
    /// </summary>
    public const string SYSTEM_NAME = "DiscountRequirement.SpendAmountOver";

    /// <summary>
    /// The key of the settings to save spend amount over
    /// </summary>
    public const string SETTINGS_KEY = "DiscountRequirement.SpendAmountOver-{0}";

    /// <summary>
    /// The key of the settings to save vendors product need to exclude
    /// </summary>
    public const string VENDORS_SETTINGS_KEY = "DiscountRequirement.SpendAmountOverExcludedVendors-{0}";

    /// <summary>
    /// The HTML field prefix for discount requirements
    /// </summary>
    public const string HTML_FIELD_PREFIX = "DiscountRulesSpendAmountOver{0}";

    /// <summary>
    /// Gets the configuration route name.
    /// </summary>
    public static string ConfigurationRouteName => "DiscountRequirement.SpendAmountOver.Configure";
}
