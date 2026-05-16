namespace Nop.Plugin.DiscountRules.MustNotCustomerRoles;

/// <summary>
/// Represents constants for the discount requirement rule.
/// </summary>
public static class DiscountRequirementDefaults
{
    /// <summary>
    /// The system name of the discount requirement rule.
    /// </summary>
    public static string SYSTEM_NAME = "DiscountRequirement.MustNotBeAssignedToCustomerRole";

    /// <summary>
    /// The key of the settings to save restricted customer roles.
    /// </summary>
    public static string SETTINGS_KEY = "DiscountRequirement.MustNotBeAssignedToCustomerRole-{0}";

    /// <summary>
    /// The HTML field prefix for discount requirements.
    /// </summary>
    public static string HTML_FIELD_PREFIX = "DiscountRulesMustNotCustomerRoles{0}";

    /// <summary>
    /// Gets the configuration route name.
    /// </summary>
    public static string ConfigurationRouteName => "DiscountRequirement.MustNotBeAssignedToCustomerRole.Configure";
}
