using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Models
{
    /// <summary>
    /// Represents a search model
    /// </summary>
    public partial record SearchModel : BaseNopModel
    {

        #region Ctor
        public SearchModel()
        {
            AvailableMake = new List<SelectListItem>();
            AvailableType = new List<SelectListItem>();
            AvailableModel = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            CatalogProductsModel = new CatalogProductsModel();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Query string
        /// </summary>
        [NopResourceDisplayName("Search.SearchTerm")]
        public string make_ { get; set; }

        [NopResourceDisplayName("Search.SearchTerm")]
        public string q { get; set; }

        /// <summary>
        /// Category 
        /// </summary>
        [NopResourceDisplayName("Search.Category")]
        public string type_ { get; set; }

        [NopResourceDisplayName("Search.IncludeSubCategories")]
        public string model_ { get; set; }
        [NopResourceDisplayName("Search.Category")]
        public int cid { get; set; }
        [NopResourceDisplayName("Search.IncludeSubCategories")]
        public bool isc { get; set; }
        [NopResourceDisplayName("Search.Manufacturer")]
        public int mid { get; set; }

        /// <summary>
        /// Vendor 
        /// </summary>
        [NopResourceDisplayName("Search.Vendor")]
        public int vid { get; set; }

        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [NopResourceDisplayName("Search.SearchInDescriptions")]
        public bool sid { get; set; }

        /// <summary>
        /// A value indicating whether "advanced search" is enabled
        /// </summary>
        [NopResourceDisplayName("Search.AdvancedSearch")]
        public bool advs { get; set; }

        /// <summary>
        /// A value indicating whether "allow search by vendor" is enabled
        /// </summary>
        public bool asv { get; set; }

        public CatalogProductsModel CatalogProductsModel { get; set; }
        public IList<SelectListItem> AvailableMake { get; set; }
        public IList<SelectListItem> AvailableType { get; set; }
        public IList<SelectListItem> AvailableModel { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        #endregion

    }
}