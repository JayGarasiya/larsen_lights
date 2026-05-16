using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data.Builders
{
    /// <summary>
    /// Represents a model product record builder
    /// </summary>
    public class ModelProductRecordBuilder : NopEntityBuilder<ModelProduct>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ModelProduct.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(ModelProduct.MakeName)).AsString(200).NotNullable()
                .WithColumn(nameof(ModelProduct.TypeName)).AsString(200).NotNullable();
        }
    }
}