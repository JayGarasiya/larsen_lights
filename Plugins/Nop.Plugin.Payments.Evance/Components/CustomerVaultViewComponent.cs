using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Payments.Evance.Components
{
    /// <summary>
    /// Represents customer vault view component
    /// </summary>
    public class CustomerVaultViewComponent : NopViewComponent
    {
        #region Fields

        private readonly EvanceSettings _evanceSettings;

        #endregion

        #region Ctor

        public CustomerVaultViewComponent(EvanceSettings evanceSettings)
        {
            _evanceSettings = evanceSettings;
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
        public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone != PublicWidgetZones.AccountNavigationAfter)
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));

            if (!_evanceSettings.Enable || !_evanceSettings.DisplaySavedDetails)
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));

            if (additionalData is not CustomerNavigationModel model)
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));

            return Task.FromResult<IViewComponentResult>(View("~/Plugins/Payments.Evance/Views/CustomerVault.cshtml", model));
        }

        #endregion
    }
}
