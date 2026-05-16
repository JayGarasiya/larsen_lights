using Nop.Web.Framework.Models;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents Tax exempt model
/// </summary>
public record TaxExemptModel : BaseNopModel
{
    public TaxExemptModel()
    {
        CustomerAttributeModels = new List<CustomerAttributeModel>();
    }

    #region Properties
    public string ButtonClass { get; set; }

    public string FormId { get; set; }

    public bool IsTaxExempt { get; set; }

    public bool IsGuest { get; set; }

    public bool OnePageCheckoutEnabled { get; set; }

    public IList<CustomerAttributeModel> CustomerAttributeModels { get; set; }
    #endregion
}
