using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.ShipmentTracking.Domain;

namespace Nop.Plugin.Misc.ShipmentTracking.Data
{    
    [NopMigration("2026/01/01 10:10:10:6455440", "Misc.ShipmentTracking base schema")]
    public class SchemaMigration : Migration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(AdminNote))).Exists())                
                    Create.TableFor<AdminNote>();
            else
            {
                //Delete old table to integrate new once
                Delete.Table(NameCompatibilityManager.GetTableName(typeof(AdminNote)));

                //Create new table
                Create.TableFor<AdminNote>();
            }
        }

        /// <summary>
        /// Collects the DOWN migration expressions
        /// </summary>
        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
