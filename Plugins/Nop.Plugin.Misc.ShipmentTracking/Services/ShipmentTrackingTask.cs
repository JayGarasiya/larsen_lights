using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
namespace Nop.Plugin.Misc.ShipmentTracking.Services
{
    /// <summary>
    /// Represents a shipment tracking task
    /// </summary>
    public class ShipmentTrackingTask : IScheduleTask
    {
        #region Fields

        protected readonly IPluginService _pluginService;
        protected readonly IShipmentTrackingService _shipmentTrackingService;
        protected readonly ILogger _logger;

        #endregion

        #region Ctor

        public ShipmentTrackingTask(IPluginService pluginService,
            IShipmentTrackingService shipmentTrackingService,
            ILogger logger)
        {
            _pluginService = pluginService;
            _shipmentTrackingService = shipmentTrackingService;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var descriptorBySystemName = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(ShipmentTrackingDefaults.SystemName, LoadPluginsMode.InstalledOnly);
            if (descriptorBySystemName == null)
                return;
            try
            {
                await _shipmentTrackingService.TrackShipmentAsync();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Shipment Tracking: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
