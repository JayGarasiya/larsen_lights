using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data.Builders
{
    /// <summary>
    /// Represents a product import record builder
    /// </summary>
    public class ProductImportRecordBuilder : NopEntityBuilder<ProductImport>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProductImport.FileName)).AsString(200).NotNullable();
        }
    }
}
