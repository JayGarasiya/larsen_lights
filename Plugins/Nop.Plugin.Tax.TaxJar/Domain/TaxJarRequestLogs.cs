using Nop.Core;

namespace Nop.Plugin.Tax.TaxJar.Domain;

/// <summary>
/// Represents a taxjar request logs record
/// </summary>
public partial class TaxJarRequestLogs : BaseEntity
{
    /// <summary>
    /// Gets or sets the response status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the requested URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the request message
    /// </summary>
    public string RequestMessage { get; set; }

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string ResponseMessage { get; set; }

    /// <summary>
    /// Gets or sets request date time
    /// </summary>
    public DateTime CreatedDateUtc { get; set; }
}
