namespace Nop.Plugin.Tax.TaxJar.Domain;

/// <summary>
/// Represents a taxjar tax exclusion on method record
/// </summary>
public partial class TaxExclusionOn
{
    #region Properties
    /// <summary>
    /// Gets or sets the controller name
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// Gets or sets the action name
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// Gets or sets the Method
    /// </summary>
    public string Method { get; set; }
    #endregion
}