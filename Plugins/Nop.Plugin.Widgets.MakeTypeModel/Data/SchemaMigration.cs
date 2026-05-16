using FluentMigrator;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.MakeTypeModel.Domain;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.PriceImport;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;

namespace Nop.Plugin.Widgets.MakeTypeModel.Data
{
    [NopMigration("2025/05/03 08:40:55:1687541", "Widgets.MakeTypeModel base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Fields

        protected IMigrationManager _migrationManager;
        private readonly INopDataProvider _nopDataProvider;

        #endregion

        #region Ctor

        public SchemaMigration(IMigrationManager migrationManager, INopDataProvider nopDataProvider)
        {
            _migrationManager = migrationManager;
            _nopDataProvider = nopDataProvider;
        }

        #endregion

        #region Methods

        public override void Up()
        {
            Create.TableFor<MakeProduct>();
            Create.TableFor<TypeProduct>();
            Create.TableFor<ModelCategory>();
            Create.TableFor<ModelProduct>();
            Create.TableFor<ModelProductMapping>();
            Create.TableFor<GranitProductImport>();
            Create.TableFor<ProductImport>();
            Create.TableFor<GranitDataImport>();
            Create.TableFor<GranitAttrImport>();
            Create.TableFor<PriceImport>();
            Create.TableFor<RetuenRequestNote>();

            _nopDataProvider.ExecuteNonQueryAsync(MakeTypeModelDefaults.StockUpdateStoredProcedure);
        }

        #endregion
    }
}

