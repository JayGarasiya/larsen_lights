using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents a configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Ctor
    public ConfigurationModel()
    {
        TestAddress = new AddressModel();
        TaxOriginAddressTypes = new List<SelectListItem>();
        AvailableCustomerAttribute = new List<SelectListItem>();
        IsTaxExempt = new List<int>();
        TaxExclusionOnModel = new TaxExclusionModel();
        TaxExclusionOn = new List<TaxExclusionModel>();
        AddressAttributes = new List<SelectListItem>();
        ExcludeTaxOnStateProvinceIds = new List<int>();
        AvailableStates = new List<SelectListItem>();
        AvailableCustomerRoles = new List<SelectListItem>();
        TaxJarRequestLogSearchModel = new TaxJarRequestLogSearchModel();

    }
    #endregion

    #region Properties
    public bool IsConfigured { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ApiToken")]
    public string ApiToken { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.IsTaxExempt")]
    public IList<int> IsTaxExempt { get; set; }
    public IList<SelectListItem> AvailableCustomerAttribute { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.PaymentMethods")]
    public string PaymentMethods { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.CommitTransactions")]
    public bool CommitTransactions { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ValidateAddress")]
    public bool ValidateAddress { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ValidateBilling")]
    public bool ValidateBilling { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ValidateShipping")]
    public bool ValidateShipping { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.OnlyNewAddress")]
    public bool OnlyNewAddress { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.AttachPdfInvoiceToOrderRefundSelectedEmail")]
    public bool AttachPdfInvoiceToOrderRefundSelectedEmail { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.CCAttributeId")]
    public int CCAttributeId { get; set; }
    public IList<SelectListItem> AddressAttributes { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.TaxOriginAddressType")]
    public int TaxOriginAddressTypeId { get; set; }
    public IList<SelectListItem> TaxOriginAddressTypes { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ExcludeTaxOnStateProvinces")]
    public IList<int> ExcludeTaxOnStateProvinceIds { get; set; }
    public IList<SelectListItem> AvailableStates { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.TaxExclusionOnCall")]
    public bool TaxExclusionOnCall { get; set; }

    [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.HiddenCustomerRoleId")]
    public int HiddenCustomerRoleId { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public TaxExclusionModel TaxExclusionOnModel { get; set; }
    public List<TaxExclusionModel> TaxExclusionOn { get; set; }
    
    public AddressModel TestAddress { get; set; }

    public string TestTaxResult { get; set; }

    public TaxJarRequestLogSearchModel TaxJarRequestLogSearchModel { get; set; }

    public bool HideGeneralBlock { get; set; }

    public bool HideLogBlock { get; set; }

    #region Nested Classes
    public class TaxExclusionModel
    {
        public TaxExclusionModel()
        {
            AvailableMethod = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ControllerName")]
        public string ControllerName { get; set; }

        [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.ActionName")]
        public string ActionName { get; set; }

        [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.Method")]
        public string Method { get; set; }
        public IList<SelectListItem> AvailableMethod { get; set; }
    }
    #endregion
    #endregion
}