using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Evance.Components
{
    /// <summary>
    /// Represents error validation view component
    /// </summary>
    public class ErrorValidationViewComponent : NopViewComponent
    {
        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View("~/Plugins/Payments.Evance/Views/ErrorValidation.cshtml"));
        }

        #endregion
    }
}
