using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ShipmentTracking.Domain;
using System.Data;

namespace Nop.Plugin.Misc.ShipmentTracking.Data
{
    /// <summary>
    /// Represents an admin note builder
    /// </summary>
    public partial class AdminNoteBuilder : NopEntityBuilder<AdminNote>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AdminNote.OrderId)).AsInt32().ForeignKey<Order>().OnDelete(Rule.None)
                .WithColumn(nameof(AdminNote.OrderNoteId)).AsInt32().ForeignKey<OrderNote>();
        }

        #endregion
    }
}