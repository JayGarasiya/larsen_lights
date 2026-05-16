using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.ProductExtension.Domain;

namespace Nop.Plugin.Widgets.ProductExtension.Data
{
    [NopMigration("2021/04/03 08:40:55:1687541", "Widgets.ProductExtension base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// <remarks>
        /// We use an explicit table creation order instead of an automatic one
        /// due to problems creating relationships between tables
        /// </remarks>
        /// </summary>
        public override void Up()
        {
            Create.TableFor<ProductNote>();
            Create.TableFor<ProductAttributeValueCondition>();
        }
    }
}