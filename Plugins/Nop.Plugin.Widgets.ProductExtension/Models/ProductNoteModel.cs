using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.ProductExtension.Models
{
    /// <summary>
    /// Represents a ProductNote model
    /// </summary>
    public partial record ProductNoteModel : BaseNopEntityModel
    {
        #region Ctor

        public ProductNoteModel()
        {
            AvailableWidgetZones = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.WidgetZone")]
        public string WidgetZone { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.DisplayOrder")]
        public virtual int DisplayOrder { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.ProductExtension.ProductNote.Fields.ShowDisplayName")]
        public bool ShowDisplayName { get; set; }
        
        #endregion
    }
}