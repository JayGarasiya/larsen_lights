using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Inventory.Order.Domain;

namespace Nop.Plugin.Misc.Inventory.Order.Data
{
    [NopMigration("2023/07/25 12:00:00", "Inventory.Order base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<OrderAssociatedProductMap>();
            Create.TableFor<PoOrder>();
            Create.TableFor<PoOrderItem>();
            Create.TableFor<ProductDimensions>();
        }

        #endregion
    }
}
