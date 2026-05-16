using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.Fulfillment.Domain;

namespace Nop.Plugin.Widgets.Fulfillment.Data
{
    /// <summary>
    /// Represents a 3Pl order entity builder
    /// </summary>
    public class ThreePlOrderBuilder : NopEntityBuilder<ThreePlOrder>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ThreePlOrder.ThreePlId)).AsInt32().ForeignKey<ThreePlRecord>();
        }
    }
}
