using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.AdminNote
{
    /// <summary>
    /// Represents an admin note search model
    /// </summary>
    public partial record AdminNoteSearchModel : OrderNoteSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.AdminNote.Fields.SearchNote")]
        public string Note { get; set; }

        //order notes
        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.DisplayToCustomer")]
        public bool AddOrderNoteDisplayToCustomer { get; set; }

        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.Note")]
        public string AddOrderNoteMessage { get; set; }

        public bool AddOrderNoteHasDownload { get; set; }
        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.Download")]
        [UIHint("Download")]
        public int AddOrderNoteDownloadId { get; set; }

        #endregion
    }
}
