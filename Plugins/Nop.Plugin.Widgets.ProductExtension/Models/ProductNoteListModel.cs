using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Models
{
    /// <summary>
    /// Represents a ProductNote list model
    /// </summary>
    public partial record ProductNoteListModel : BasePagedListModel<ProductNoteModel>
    {
    }
}