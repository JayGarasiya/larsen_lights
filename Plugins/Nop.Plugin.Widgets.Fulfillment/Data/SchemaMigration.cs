using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.Fulfillment.Domain;

namespace Nop.Plugin.Widgets.Fulfillment.Data
{
    [NopMigration("2021/02/03 08:40:55:1687541", "Misc.FullFilment base schema", MigrationProcessType.Installation)]
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
            Create.TableFor<ThreePlRecord>();
            Create.TableFor<ThreePlOrder>();
            Create.TableFor<ThreePlShippingMethod>();
        }
    }
}
