using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.ShipmentTracking.Data
{
    /// <summary>
    /// Data migration for the ShipmentTracking plugin.
    /// Adds or updates localization resources required for the plugin.
    /// </summary>
    [NopMigration("2026/02/02 16:11:07:6455407", "Misc.ShipmentTracking", MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        
        #endregion

        #region Ctor
        public DataMigration(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Executes the migration and adds or updates localization resources
        /// used in the ShipmentTracking plugin order search section.
        /// </summary>
        public override async void Up()
        {
           
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.ShipmentTracking.Orders.List.SearchCompany"] = "Company",
                ["Plugins.Misc.ShipmentTracking.Orders.List.SearchCompany.Hint"] = "Search by company.",
                
            });
        }

        /// <summary>
        /// Reverts the migration changes.
        /// </summary>
        public override void Down()
        {
        }
        #endregion
    }
}
