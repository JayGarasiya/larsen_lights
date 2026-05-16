using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Reports.ChartsAndTables.Factories;
using Nop.Plugin.Reports.ChartsAndTables.Models;
using Nop.Plugin.Reports.ChartsAndTables.Security;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Reports.ChartsAndTables.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class ReportsCustomerViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly ChartsAndTablesSettings _chartsAndTablesSettings;
        protected readonly IPermissionService _permissionService;
        protected readonly IChartsAndTablesModelFactory _chartsAndTablesModelFactory;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public ReportsCustomerViewComponent(ChartsAndTablesSettings chartsAndTablesSettings,
            IPermissionService permissionService,
            IChartsAndTablesModelFactory chartsAndTablesModelFactory,
            IChartsAndTablesServices chartsAndTablesServices,
            WidgetSettings widgetSettings)
        {
            _chartsAndTablesSettings = chartsAndTablesSettings;
            _permissionService = permissionService;
            _chartsAndTablesModelFactory = chartsAndTablesModelFactory;
            _chartsAndTablesServices = chartsAndTablesServices;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var licenseKey = _chartsAndTablesSettings.LicenseKey;
            var responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(licenseKey);
            if (responseCode != 100)
                return Content(string.Empty);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName))
                return Content(string.Empty);

            if (!widgetZone.Equals(AdminWidgetZones.CustomerDetailsBlock))
                return Content(string.Empty);

            if (!await _permissionService.AuthorizeAsync(ReportsPermissionProvider.ACCESS_ADMIN_ACCESS_REPORTS) &&
                !await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE) &&
                !await _permissionService.AuthorizeAsync(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE))
                return Content(string.Empty);

            var baseModel = additionalData as CustomerModel;
            if (baseModel.Id == 0)
                return Content(string.Empty);

            if (!_chartsAndTablesSettings.CustomerReport)
                return Content(string.Empty);

            var model = await _chartsAndTablesModelFactory.PrepareReportDetailCustomerSearchModelAsync(new ChartsAndTablesSearchModel());
            model.CustomerId = baseModel.Id;

            return View("~/Plugins/Reports.ChartsAndTables/Views/Customer/CustomerDetailReport.cshtml", model);
        }

        #endregion
    }
}