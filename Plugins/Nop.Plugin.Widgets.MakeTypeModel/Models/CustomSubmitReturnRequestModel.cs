using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Models
{
    /// <summary>
    /// Represents a custom submit return request model
    /// </summary>
    public partial record CustomSubmitReturnRequestModel : BaseNopModel
    {
        #region Ctor

        public CustomSubmitReturnRequestModel()
        {
            Items = new List<OrderItemModel>();
            AvailableReturnReasons = new List<ReturnRequestReasonModel>();
            AvailableReturnActions = new List<ReturnRequestActionModel>();
        }

        #endregion

        #region Properties

        public int OrderId { get; set; }
        public string CustomOrderNumber { get; set; }

        public IList<OrderItemModel> Items { get; set; }

        [NopResourceDisplayName("ReturnRequests.ReturnReason")]
        public int ReturnRequestReasonId { get; set; }
        public IList<ReturnRequestReasonModel> AvailableReturnReasons { get; set; }

        [NopResourceDisplayName("ReturnRequests.ReturnAction")]
        public int ReturnRequestActionId { get; set; }
        public IList<ReturnRequestActionModel> AvailableReturnActions { get; set; }

        [NopResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public bool AllowFiles { get; set; }
        [NopResourceDisplayName("ReturnRequests.UploadedFile")]
        public Guid UploadedFileGuid { get; set; }
        public string Result { get; set; }

        #endregion

        #region Nested classes

        public partial record OrderItemModel : BaseNopEntityModel
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }
            public IList<AttributeModel> products { get; set; }
        }

        public partial record ReturnRequestReasonModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }

        public partial record ReturnRequestActionModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }

        public partial record AttributeModel : BaseNopEntityModel
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        #endregion
    }
}
