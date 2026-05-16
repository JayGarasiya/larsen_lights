using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Models
{
    public partial record DealerPriceSelectorModel : BaseNopModel
    {
        public DealerPriceSelectorModel()
        {
            AvailablePrices = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailablePrices { get; set; }

        public int CurrentPriceId { get; set; }

        public bool ShowConfirmation { get; set; }
    }
}
