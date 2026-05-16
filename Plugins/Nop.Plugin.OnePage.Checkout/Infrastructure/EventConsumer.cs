using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;
using Nop.Core.Http;

namespace Nop.Plugin.OnePage.Checkout.Infrastructure
{
    /// <summary>
    /// Represent event consumer
    /// </summary>
    public class EventConsumer : IConsumer<PageRenderingEvent>
    {
        #region Fields

        protected readonly OnePageCheckoutSettings _onePageCheckoutSettings;
        protected readonly IPluginService _pluginService;
        protected readonly IActionContextAccessor _actionContextAccessor;
        protected readonly IUrlHelperFactory _urlHelperFactory;
        protected readonly ICustomerService _customerService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EventConsumer(OnePageCheckoutSettings onePageCheckoutSettings,
            IPluginService pluginService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _onePageCheckoutSettings = onePageCheckoutSettings;
            _pluginService = pluginService;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _customerService = customerService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get Url helper
        /// </summary>
        /// <returns>UrlHelper</returns>
        protected virtual IUrlHelper GetUrlHelper()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }

        /// <summary>
        /// Prepare login register event
        /// </summary>
        protected async Task<string> PrepareLoginRegisterEventAsync()
        {
            var urlHelper = GetUrlHelper();
            var IsAuthenticated = await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync());
            //prepare login button script
            var loginScript = $@"
                    $(document).on('click', 'a[href*=""{urlHelper.RouteUrl(NopRouteNames.General.LOGIN)}""]', function (event) {{
                        event.preventDefault();
                        $(this).attr('data-url', '{urlHelper.RouteUrl(NopRouteNames.General.LOGIN)}'); 
                        if ({IsAuthenticated.ToString().ToLower()}) {{
                            displayPopupNotification('{await _localizationService.GetResourceAsync("Plugins.OnePage.Checkout.LoginRegister.AlreadyLogin")}', 'success', false);
                        }} else {{
                                $(this).lrpopup();
                        }}
                    }});";

            //prepare register button script
            var registerScript = $@"
                        $(document).on('click', 'a[href*=""{urlHelper.RouteUrl(NopRouteNames.Standard.REGISTER)}""]', function (event) {{
                            event.preventDefault();
                            $(this).attr('data-url', '{urlHelper.RouteUrl(NopRouteNames.Standard.REGISTER)}');
                            if ({IsAuthenticated.ToString().ToLower()}) {{
                                displayPopupNotification('{await _localizationService.GetResourceAsync("Plugins.OnePage.Checkout.LoginRegister.AlreadyRegistered")}', 'success', false);
                            }} else {{
                                $(this).lrpopup();
                            }}
                        }});";

            //hash check for login /register to trigger event 
            var hasScript = $@"if (window.location.hash) {{
                    var hash = window.location.hash.substring(1);
                    if (hash == 'login' || hash == 'register') {{
                        $('a[href^=""/' + hash + '""]').trigger('click');
                    }}
                    if(hash == 'logincheckoutasguest') {{
                        var login = $('a[href^=""/login""]');
                        login.attr('href', ""{urlHelper.RouteUrl(NopRouteNames.Standard.LOGIN_CHECKOUT_AS_GUEST, new { returnUrl = urlHelper.RouteUrl(NopRouteNames.General.CART) })}"");
                        login.trigger('click');
                    }}
                }}";

            return @$"<script asp-location=""Footer"" type=""text/javascript"">
                        $(document).ready(function () {{
                            {loginScript} 
                            {registerScript}
                            {hasScript}
                        }});
                    </script>";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle page rendering event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            //check is admin area request
            var routeValues = _actionContextAccessor.ActionContext.RouteData.Values;
            var areaExist = routeValues.ContainsKey("area") ? (routeValues["area"]?.ToString() ?? string.Empty).Equals("Admin", StringComparison.InvariantCultureIgnoreCase) : false;
            if (areaExist)
                return;

            //ensure that onepage checkout is active
            var descriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(OnePageCheckOutDefaults.SystemName, LoadPluginsMode.InstalledOnly);
            if (descriptor == null)
                return;

            //ensure that login/register popup is active
            if (!_onePageCheckoutSettings.LoginRegisterPopUp && !_onePageCheckoutSettings.EnableOnePageCheckout)
                return;

            
            if(_onePageCheckoutSettings.EnableOnePageCheckout)
            {
                eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/Plugins/OnePage.Checkout/Content/js/slick-slider-1.6.0.min.js");
                eventMessage.Helper.AddCssFileParts("~/Plugins/OnePage.Checkout/Content/css/slick-slider-1.6.0.css");
            }

            //add css to one page checkout
            eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/Plugins/OnePage.Checkout/Content/js/public.floatinglabels.js");
            eventMessage.Helper.AddCssFileParts("~/Plugins/OnePage.Checkout/Content/css/styles.css");

            if (_onePageCheckoutSettings.LoginRegisterPopUp)
            {
                eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/lib_npm/magnific-popup/jquery.magnific-popup.min.js");
                eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/Plugins/OnePage.Checkout/Content/js/public.loginregister.js");
                eventMessage.Helper.AddCssFileParts("~/lib_npm/magnific-popup/magnific-popup.css");

                //prepare and append inline script to initialize click on login/register
                eventMessage.Helper.AddInlineScriptParts(ResourceLocation.Footer, await PrepareLoginRegisterEventAsync());
            }
        }

        #endregion
    }
}
