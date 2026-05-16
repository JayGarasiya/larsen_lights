using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Http;
using Nop.Data;
using Nop.Services.Plugins;
using System.Net;

namespace Nop.Plugin.OnePage.Checkout.Filters
{
    /// <summary>
    /// Represent login register url hash action filter
    /// </summary>
    public class LoginRegisterUrlHashActionFilter : ActionFilterAttribute
    {
        #region Fields

        protected readonly OnePageCheckoutSettings _onePageCheckoutSettings;
        protected readonly IPluginService _pluginService;
        protected readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public LoginRegisterUrlHashActionFilter(OnePageCheckoutSettings onePageCheckoutSettings,
            IPluginService pluginService,
            IWebHelper webHelper)
        {
            _onePageCheckoutSettings = onePageCheckoutSettings;
            _pluginService = pluginService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get the route name associated with the request rendering this page
        /// </summary>
        /// <returns>Route name</returns>
        private string GetRouteName(ActionExecutedContext context)
        {
            //or try to get a registered endpoint route name
            var endpointFeature = context.HttpContext.Features.Get<IEndpointFeature>();
            var routeNameMetadata = endpointFeature.Endpoint.Metadata.GetMetadata<RouteNameMetadata>();
            return routeNameMetadata.RouteName;
        }

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task CheckLoginRegisterHashAsync(ActionExecutedContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.HttpContext.Request == null)
                return;

            if (_webHelper.IsAjaxRequest(context.HttpContext.Request))
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //ensure that onepage checkout is active
            var descriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(OnePageCheckOutDefaults.SystemName, LoadPluginsMode.InstalledOnly);
            if (descriptor == null)
                return;

            //ensure that login/register popup is active
            if (!_onePageCheckoutSettings.LoginRegisterPopUp)
                return;

            //only in Post requests
            if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                return;

            //get action and controller names
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            var routeName = GetRouteName(context);
            if (string.IsNullOrEmpty(routeName))
                return;

            if (context.Result is ChallengeResult)
            {
                var returnUrl = _webHelper.GetRawUrl(context.HttpContext.Request);
                context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, new { returnUrl = returnUrl }, fragment: $"login");
            }

            if (routeName.Equals(NopRouteNames.General.LOGIN, StringComparison.InvariantCultureIgnoreCase) ||
                routeName.Equals(NopRouteNames.Standard.LOGIN_CHECKOUT_AS_GUEST, StringComparison.InvariantCultureIgnoreCase) ||
                routeName.Equals(NopRouteNames.Standard.REGISTER, StringComparison.InvariantCultureIgnoreCase))
            {
                var returnUrl = string.Empty;
                if (context.HttpContext.Request?.Query != null || context.HttpContext.Request.Query.Any())
                {
                    if (context.HttpContext.Request.Query.TryGetValue("returnUrl", out var url) && !StringValues.IsNullOrEmpty(url))
                        returnUrl = url;
                }

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    if (returnUrl.Contains("/Admin", StringComparison.InvariantCultureIgnoreCase))
                        context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, new { returnUrl = returnUrl }, fragment: $"{routeName.ToLower()}");
                    else
                        context.HttpContext.Response.Redirect($"{returnUrl}#{routeName.ToLower()}");
                }
                else
                    context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, null, fragment: $"{routeName.ToLower()}");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called after the action executes, before the action result.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <returns>A task that represents the operation</returns>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            CheckLoginRegisterHashAsync(context).GetAwaiter().GetResult();
        }

        #endregion
    }
}
