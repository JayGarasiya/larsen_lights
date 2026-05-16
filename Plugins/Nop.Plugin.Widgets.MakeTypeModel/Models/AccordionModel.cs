using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Models
{
    /// <summary>
    /// Represents an accordion model
    /// </summary>
    public record AccordionModel : BaseNopModel
    {
        #region Ctor
        public AccordionModel()
        {
            MakeModels = new List<MakeModel>();
        }
        #endregion

        #region Properties

        public int ProductId { get; set; }

        public IList<MakeModel> MakeModels { get; set; }

        #endregion

        #region Nested Classes

        public record MakeModel : BaseNopModel
        {
            public MakeModel()
            {
                TypeModels = new List<TypeModel>();
            }

            public string MakeName { get; set; }

            public IList<TypeModel> TypeModels { get; set; }

            #region Nested Classes

            public record TypeModel : BaseNopModel
            {
                public TypeModel()
                {
                    ModelsProduct = new List<ModelsProductModel>();
                }

                public string TypeName { get; set; }

                public IList<ModelsProductModel> ModelsProduct { get; set; }

                #region Nested Classes

                public record ModelsProductModel : BaseNopModel
                {
                    public string Name { get; set; }

                    public string Description { get; set; }

                    public string QueryString { get; set; }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
