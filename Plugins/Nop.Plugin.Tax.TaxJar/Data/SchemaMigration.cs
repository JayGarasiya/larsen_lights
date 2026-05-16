using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Tax.TaxJar.Domain;

namespace Nop.Plugin.Tax.TaxJar.Data;

[NopMigration("2021/04/03 08:40:55:1687541", "Widgets.TaxJar base schema", MigrationProcessType.Installation)]
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
        Create.TableFor<TaxJarRequestLogs>();
    }
}
