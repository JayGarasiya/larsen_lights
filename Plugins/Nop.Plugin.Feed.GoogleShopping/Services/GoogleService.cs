using Nop.Data;
using Nop.Plugin.Feed.GoogleShopping.Domain;

namespace Nop.Plugin.Feed.GoogleShopping.Services;

/// <summary>
/// Represents the Google Service that handles operations related to Google product records.
/// </summary>
public partial class GoogleService : IGoogleService
{
    #region Fields
    private readonly IRepository<GoogleProductRecord> _gpRepository;
    #endregion

    #region Ctor
    public GoogleService(IRepository<GoogleProductRecord> gpRepository)
    {
        _gpRepository = gpRepository;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Retrieves the content of an embedded file asynchronously.
    /// </summary>
    /// <param name="resourceName">The name of the embedded resource file.</param>
    /// <returns>Content of the file as a string.</returns>
    private async Task<string> GetEmbeddedFileContentAsync(string resourceName)
    {
        var fullResourceName = $"Nop.Plugin.Feed.GoogleShopping.Files.{resourceName}";
        var assem = GetType().Assembly;
        using var stream = assem.GetManifestResourceStream(fullResourceName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Deletes a Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to delete.</param>
    public virtual async Task DeleteGoogleProductAsync(GoogleProductRecord googleProductRecord)
    {
        ArgumentNullException.ThrowIfNull(googleProductRecord);

        await _gpRepository.DeleteAsync(googleProductRecord);
    }

    /// <summary>
    /// Retrieves all Google product records.
    /// </summary>
    /// <returns>A list of all Google product records.</returns>
    public virtual async Task<IList<GoogleProductRecord>> GetAllAsync()
    {
        var query = from gp in _gpRepository.Table
                    orderby gp.Id
                    select gp;
        var records = await query.ToListAsync();
        return records;
    }

    /// <summary>
    /// Retrieves a Google product record by its unique ID.
    /// </summary>
    /// <param name="googleProductRecordId">The ID of the Google product record to retrieve.</param>
    /// <returns>The corresponding Google product record, or null if not found.</returns>
    public virtual async Task<GoogleProductRecord> GetByIdAsync(int googleProductRecordId)
    {
        if (googleProductRecordId == 0)
            return null;

        return await _gpRepository.GetByIdAsync(googleProductRecordId);
    }

    /// <summary>
    /// Retrieves a Google product record by its associated product ID.
    /// </summary>
    /// <param name="productId">The product ID associated with the Google product record.</param>
    /// <returns>The corresponding Google product record, or null if not found.</returns>
    public virtual async Task<GoogleProductRecord> GetByProductIdAsync(int productId)
    {
        if (productId == 0)
            return null;

        var query = from gp in _gpRepository.Table
                    where gp.ProductId == productId
                    orderby gp.Id
                    select gp;
        var record = await query.FirstOrDefaultAsync();
        return record;
    }

    /// <summary>
    /// Inserts a new Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to insert.</param>
    public virtual async Task InsertGoogleProductRecordAsync(GoogleProductRecord googleProductRecord)
    {
        ArgumentNullException.ThrowIfNull(googleProductRecord);

        await _gpRepository.InsertAsync(googleProductRecord);
    }

    /// <summary>
    /// Updates an existing Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to update.</param>
    public virtual async Task UpdateGoogleProductRecordAsync(GoogleProductRecord googleProductRecord)
    {
        ArgumentNullException.ThrowIfNull(googleProductRecord);

        await _gpRepository.UpdateAsync(googleProductRecord);
    }

    /// <summary>
    /// Retrieves a list of taxonomy entries from an embedded taxonomy file.
    /// </summary>
    /// <returns>A list of taxonomy entries.</returns>
    public virtual async Task<IList<string>> GetTaxonomyListAsync()
    {
        var fileContent = await GetEmbeddedFileContentAsync("taxonomy.txt");
        if (string.IsNullOrEmpty(fileContent))
            return new List<string>();

        // Parse the file into a list of entries
        var result = fileContent.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries).ToList();
        return result;
    }
    #endregion
}
