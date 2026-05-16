using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Tax.TaxJar.Controllers;

public class TaxJarRequestLogController : BaseAdminController
{
    #region Fields
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly TaxJarRequestLogService _taxJarRequestLogService;
    #endregion

    #region Ctor
    public TaxJarRequestLogController(ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IHtmlFormatter htmlFormatter,
        ILocalizationService localizationService,
        INotificationService notificationService,
        TaxJarRequestLogService taxJarRequestLogService)
    {
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _htmlFormatter = htmlFormatter;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _taxJarRequestLogService = taxJarRequestLogService;
    }
    #endregion

    #region Methods
    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> LogList(TaxJarRequestLogSearchModel searchModel)
    {
        //prepare filter parameters
        var createdFromValue = searchModel.CreatedFrom.HasValue
            ? _dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()) as DateTime? 
            : null;
        var createdToValue = searchModel.CreatedTo.HasValue
            ? _dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1) as DateTime?
            : null;

        //get taxjar transaction log
        var taxtransactionLog = await _taxJarRequestLogService.GetTaxJarRequestLogAsync(createdFromUtc: createdFromValue,
                                                                                        createdToUtc: createdToValue,
                                                                                        pageIndex: searchModel.Page - 1,
                                                                                        pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new TaxJarRequestLogListModel().PrepareToGridAsync(searchModel, taxtransactionLog, () =>
        {
            return taxtransactionLog.SelectAwait(async logItem => new TaxJarRequestLogModel
            {
                Id = logItem.Id,
                StatusCode = logItem.StatusCode,
                Url = logItem.Url,
                CustomerId = logItem.CustomerId,
                CreatedDate = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedDateUtc, DateTimeKind.Utc)
            });
        });

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _taxJarRequestLogService.DeleteTaxJarRequestLogAsync([.. selectedIds]);

        return Json(new { Result = true });
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> View(int id)
    {
        //try to get log item with the passed identifier
        var logItem = await _taxJarRequestLogService.GetTaxJarRequestLogByIdAsync(id);
        if (logItem == null)
            return RedirectToAction("Configure", "TaxJar");

        var model = new TaxJarRequestLogModel
        {
            Id = logItem.Id,
            StatusCode = logItem.StatusCode,
            Url = logItem.Url,
            RequestMessage = _htmlFormatter.FormatText(logItem.RequestMessage, false, true, false, false, false, false),
            ResponseMessage = _htmlFormatter.FormatText(logItem.ResponseMessage, false, true, false, false, false, false),
            CustomerId = logItem.CustomerId,
            CustomerEmail = (await _customerService.GetCustomerByIdAsync(logItem.CustomerId))?.Email,
            CreatedDate = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedDateUtc, DateTimeKind.Utc)
        };

        return View("~/Plugins/Tax.TaxJar/Views/Log/View.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> Delete(int id)
    {
        //try to get log item with the passed identifier
        var logItem = await _taxJarRequestLogService.GetTaxJarRequestLogByIdAsync(id);
        if (logItem != null)
        {
            await _taxJarRequestLogService.DeleteTaxJarRequestLogAsync(logItem);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.Avalara.Log.Deleted"));
        }

        return RedirectToAction("Configure", "TaxJar");
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> ClearAll()
    {
        await _taxJarRequestLogService.ClearLogAsync();

        return Json(new { Result = true });
    }
    #endregion
}
