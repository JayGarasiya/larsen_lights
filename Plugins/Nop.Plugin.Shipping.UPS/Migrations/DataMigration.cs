using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Plugin.Shipping.UPS.Migrations
{
    [NopMigration("2026/02/06 11:00:00", "Shipping.UPS base schema", MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        #region Fields
        protected readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public DataMigration(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public async override void Up()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Shipping.UPS.Fields.HSCode"] = "HS Code",
                ["Plugins.Shipping.UPS.Fields.HSCode.Hint"] = "Enter HS code for duty cost.",
                ["Plugins.Shipping.UPS.ShippingOption.Description"] = "<span style='font-weight:700;'>{0}</span> this is your expected duty, please pay from the tracking link when you receive the tracking number. If you do not agree to pay the duty, do not place the order."
            });
        }

        public override void Down()
        {
        }

        #endregion
    }
}
