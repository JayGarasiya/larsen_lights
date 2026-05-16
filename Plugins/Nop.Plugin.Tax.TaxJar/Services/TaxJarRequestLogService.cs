using Nop.Core;
using Nop.Data;
using Nop.Plugin.Tax.TaxJar.Domain;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// Represents the taxjar request log service implementation
/// </summary>
public class TaxJarRequestLogService
{
    #region Fields
    private readonly IRepository<TaxJarRequestLogs> _taxjarRequestLogRepository;
    #endregion

    #region Ctor
    public TaxJarRequestLogService(IRepository<TaxJarRequestLogs> taxjarRequestLogRepository)
    {
        _taxjarRequestLogRepository = taxjarRequestLogRepository;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get tax transaction log
    /// </summary>
    /// <param name="customerId">Customer identifier; pass null to load all records</param>
    /// <param name="createdFromUtc">Log item creation from; pass null to load all records</param>
    /// <param name="createdToUtc">Log item creation to; pass null to load all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of tax transaction log items
    /// </returns>
    public async Task<IPagedList<TaxJarRequestLogs>> GetTaxJarRequestLogAsync(
        int? customerId = null,
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        // Get all logs
        var query = _taxjarRequestLogRepository.Table;

        // Filter by customer
        if (customerId.HasValue)
            query = query.Where(logItem => logItem.CustomerId == customerId);

        // Filter by dates
        if (createdFromUtc.HasValue)
            query = query.Where(logItem => logItem.CreatedDateUtc >= createdFromUtc.Value);
        if (createdToUtc.HasValue)
            query = query.Where(logItem => logItem.CreatedDateUtc <= createdToUtc.Value);

        // Order log records
        query = query.OrderByDescending(logItem => logItem.CreatedDateUtc).ThenByDescending(logItem => logItem.Id);

        // Return paged log
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Get a log item by the identifier
    /// </summary>
    /// <param name="logItemId">Log item identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the log item
    /// </returns>
    public async Task<TaxJarRequestLogs> GetTaxJarRequestLogByIdAsync(int logItemId)
    {
        return await _taxjarRequestLogRepository.GetByIdAsync(logItemId);
    }

    /// <summary>
    /// Insert the log item
    /// </summary>
    /// <param name="logItem">Log item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertTaxJarRequestLogAsync(TaxJarRequestLogs logItem)
    {
        ArgumentNullException.ThrowIfNull(logItem);

        await _taxjarRequestLogRepository.InsertAsync(logItem, false);
    }

    /// <summary>
    /// Delete the log item
    /// </summary>
    /// <param name="logItem">Log item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteTaxJarRequestLogAsync(TaxJarRequestLogs logItem)
    {
        await _taxjarRequestLogRepository.DeleteAsync(logItem, false);
    }

    /// <summary>
    /// Delete log items
    /// </summary>
    /// <param name="ids">Log items identifiers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteTaxJarRequestLogAsync(int[] ids)
    {
        await _taxjarRequestLogRepository.DeleteAsync(logItem => ids.Contains(logItem.Id));
    }

    /// <summary>
    /// Clear the log
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ClearLogAsync()
    {
        await _taxjarRequestLogRepository.TruncateAsync();
    }
    #endregion
}
