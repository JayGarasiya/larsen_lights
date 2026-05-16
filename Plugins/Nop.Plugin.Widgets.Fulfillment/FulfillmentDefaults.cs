namespace Nop.Plugin.Widgets.Fulfillment
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class FulfillmentDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Widgets.Fulfillment";

        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string OldSystemName => "Misc.FullFilment";

        /// <summary>
        /// Gets a type of the 3PL Fulfillment process task
        /// </summary>
        public static string FulfillmentSendOldTask => "Nop.Plugin.Misc.FullFilment.Services.Tasks.FullFilmentSendTask, Nop.Plugin.Misc.FullFilment";

        /// <summary>
        /// Gets a name, type and period (in seconds) of the 3PL Fulfillment process task
        /// </summary>
        public static (string Name, string Type, int Period) FulfillmentSendTask =>
        (
            "Process Task (3PL Fulfillment)",
            "Nop.Plugin.Widgets.Fulfillment.ScheduleTasks.FulfillmentSendTask, Nop.Plugin.Widgets.Fulfillment",
            1200
        );

        /// <summary>
        /// Gets a type of the 3PL Fulfillment update inventory task
        /// </summary>
        public static string FulfillmentInventoryOldTask => "Nop.Plugin.Misc.FullFilment.Services.Tasks.FullFilmentInventoryTask, Nop.Plugin.Misc.FullFilment";

        /// <summary>
        /// Gets a name, type and period (in seconds) of the 3PL Fulfillment update inventory task
        /// </summary>
        public static (string Name, string Type, int Period) FulfillmentInventoryTask =>
        (
            "Update Inventory (3PL Fulfillment)",
            "Nop.Plugin.Widgets.Fulfillment.ScheduleTasks.FulfillmentInventoryTask, Nop.Plugin.Widgets.Fulfillment",
            86400
        );
        /// <summary>
        /// Gets a type of the 3PL Fulfillment update tracking number task
        /// </summary>
        public static string FulfillmentTrackingOldTask => "Nop.Plugin.Misc.FullFilment.Services.Tasks.FullFilmentTrackingTask, Nop.Plugin.Misc.FullFilment";

        /// <summary>
        /// Gets a name, type and period (in seconds) of the 3PL Fulfillment update tracking number task
        /// </summary>
        public static (string Name, string Type, int Period) FulfillmentTrackingTask =>
        (
            "Update Tracking Number (3PL Fulfillment)",
            "Nop.Plugin.Widgets.Fulfillment.ScheduleTasks.FulfillmentTrackingTask, Nop.Plugin.Widgets.Fulfillment",
            1600
        );
    }
}
