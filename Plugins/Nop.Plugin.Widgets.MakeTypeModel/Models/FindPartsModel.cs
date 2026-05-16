using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Models
{
    /// <summary>
    /// Represents a find parts model
    /// </summary>
    public record FindPartsModel : BaseNopModel
    {

        #region Ctor
        public FindPartsModel()
        {
            AvailableMake = new List<SelectListItem>();
            AvailableType = new List<SelectListItem>();
            AvailableModel = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Make
        /// </summary>
        [NopResourceDisplayName("Plugin.Widgets.MakeTypeModel.FindParts.Make")]
        public string Make { get; set; }
        public IList<SelectListItem> AvailableMake { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [NopResourceDisplayName("Plugin.Widgets.MakeTypeModel.FindParts.Type")]
        public string Type { get; set; }
        public IList<SelectListItem> AvailableType { get; set; }

        /// <summary>
        /// Model 
        /// </summary>
        [NopResourceDisplayName("Plugin.Widgets.MakeTypeModel.FindParts.Model")]
        public string Model { get; set; }
        public IList<SelectListItem> AvailableModel { get; set; }

        /// <summary>
        /// Category 
        /// </summary>
        [NopResourceDisplayName("Plugin.Widgets.MakeTypeModel.FindParts.Category")]
        public int Category { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        #endregion
    }
}
