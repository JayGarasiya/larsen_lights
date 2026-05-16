using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.ShipmentTracking.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //override some of default routes in Admin area
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.Orders", "Admin/Order/List",
                new { controller = "OrderShipment", action = "List", area = AreaNames.ADMIN });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.Shipments", "Admin/Order/ShipmentList",
                new { controller = "OrderShipment", action = "ShipmentList", area = AreaNames.ADMIN });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.Orders", $"Admin/Order/ShipmentsByOrder",
                new { controller = "OrderShipment", action = "ShipmentsByOrder", area = AreaNames.ADMIN });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.ShipmentDetails", $"Admin/Order/ShipmentDetails/{{id?}}",
                new { controller = "OrderShipment", action = "ShipmentDetails", area = AreaNames.ADMIN });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.Orders", $"Admin/Order/AddShipment/{{id?}}",
                new { controller = "OrderShipment", action = "AddShipment", area = AreaNames.ADMIN,  });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.AddOrderNotes", $"Admin/Order/OrderNoteAdd",
                new { controller = "OrderShipment", action = "OrderNoteAdd", area = AreaNames.ADMIN, });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.ShipmentTracking.DeleteOrderNotes", $"Admin/Order/OrderNoteDelete",
                new { controller = "OrderShipment", action = "OrderNoteDelete", area = AreaNames.ADMIN, });
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1; //set a value that is greater than the default one in Nop.Web to override routes

        #endregion
    }
}