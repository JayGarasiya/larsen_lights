using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.Fulfillment.Domain;

namespace Nop.Plugin.Widgets.Fulfillment.Data
{
    /// <summary>
    /// Represents a 3Pl shipping method entity builder
    /// </summary>
    public class ThreePlShippingMethodBuilder : NopEntityBuilder<ThreePlShippingMethod>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ThreePlShippingMethod.ShippingMethod)).AsString(200).NotNullable()
                .WithColumn(nameof(ThreePlShippingMethod.ThreePlCarrier)).AsString(200).NotNullable()
                .WithColumn(nameof(ThreePlShippingMethod.ThreePlService)).AsString(200).NotNullable();
        }
    }
}
