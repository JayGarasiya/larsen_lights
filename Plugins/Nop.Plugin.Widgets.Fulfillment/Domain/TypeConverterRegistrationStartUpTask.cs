using System.ComponentModel;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders;

namespace Nop.Plugin.Widgets.Fulfillment.Domain
{
    /// <summary>
    /// Startup task for the registration custom type converters
    /// </summary>
    public class TypeConverterRegistrationStartUpTask : IStartupTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            //Sync order items
            TypeDescriptor.AddAttributes(typeof(SyncOrderItem), new TypeConverterAttribute(typeof(SyncOrderItemTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(List<SyncOrderItem>), new TypeConverterAttribute(typeof(SyncOrderItemListTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(IList<SyncOrderItem>), new TypeConverterAttribute(typeof(SyncOrderItemListTypeConverter)));
        }

        /// <summary>
        /// Gets order of this startup task implementation
        /// </summary>
        public int Order => 1;
    }
}
