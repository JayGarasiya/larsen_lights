using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.Fulfillment.Domain;

namespace Nop.Plugin.Widgets.Fulfillment.Data
{
    /// <summary>
    /// Represents a 3Pl record entity builder
    /// </summary>
    public class ThreePlRecordBuilder : NopEntityBuilder<ThreePlRecord>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ThreePlRecord.OrderId)).AsInt32().ForeignKey<Order>();
        }
    }
}
