using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data.Builders
{
    /// <summary>
    /// Represents a type product record builder
    /// </summary>
    public class TypeProductRecordBuilder : NopEntityBuilder<TypeProduct>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TypeProduct.Name)).AsString(200).NotNullable();
        }
    }
}