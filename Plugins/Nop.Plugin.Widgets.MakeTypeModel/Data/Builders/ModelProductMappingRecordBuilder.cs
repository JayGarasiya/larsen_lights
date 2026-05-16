using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data.Builders
{
    /// <summary>
    /// Represents a model product mapping record builder
    /// </summary>
    public class ModelProductMappingRecordBuilder : NopEntityBuilder<ModelProductMapping>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ModelProductMapping.ModelId)).AsInt32().ForeignKey<ModelProduct>()
                .WithColumn(nameof(ModelProductMapping.ProductId)).AsInt32().ForeignKey<Product>();
        }
    }
}