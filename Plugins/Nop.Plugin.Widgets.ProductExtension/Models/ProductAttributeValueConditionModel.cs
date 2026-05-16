using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Models
{
    /// <summary>
    /// Represents product attribute value condition model
    /// </summary>
    public partial record ProductAttributeValueConditionModel : BaseNopEntityModel
    {
        public ProductAttributeValueConditionModel()
        {
            Values = new List<ProductAttributeValueModel>();
        }

        public string Name { get; set; }

        public int ProductAttributeMappingId { get; set; }

        public string TextPrompt { get; set; }

        public IList<ProductAttributeValueModel> Values { get; set; }

        #region Nested classes

        public partial record ProductAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }
}
