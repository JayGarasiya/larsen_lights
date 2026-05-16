using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.Evance.Domain;

namespace Nop.Plugin.Payments.Evance.Migrations
{
    [NopMigration("2025/04/01 04:03:00", "Payments.Evance base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CustomerVault))).Exists())
                Create.TableFor<CustomerVault>();
        }

        #endregion
    }
}
