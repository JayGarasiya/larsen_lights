using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data
{
    /// <summary>
    /// Represents a three pl record builder
    /// </summary>
    public class ThreePlRecordBuilder : NopEntityBuilder<ModelCategory>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ModelCategory.CategoryId)).AsInt32().ForeignKey<Category>();
        }
    }
}