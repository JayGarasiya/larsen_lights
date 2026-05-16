using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models
{
    /// <summary>
    /// Represents an override return request model 
    /// </summary>
    public partial record OverrideReturnRequestModel : ReturnRequestModel
    {
        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.SKU")]
        public string SKU { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.Note")]
        public string Note { get; set; }
    }
}
