using Nop.Plugin.Feed.GoogleShopping.Domain;

namespace Nop.Plugin.Feed.GoogleShopping.Services;

/// <summary>
/// Represents the contract for the Google Service
/// </summary>
public partial interface IGoogleService
{
    /// <summary>
    /// Deletes a Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to delete.</param>
    Task DeleteGoogleProductAsync(GoogleProductRecord googleProductRecord);

    /// <summary>
    /// Retrieves all Google product records.
    /// </summary>
    /// <returns>A list of all Google product records.</returns>
    Task<IList<GoogleProductRecord>> GetAllAsync();

    /// <summary>
    /// Retrieves a Google product record by its unique ID.
    /// </summary>
    /// <param name="googleProductRecordId">The ID of the Google product record to retrieve.</param>
    /// <returns>The corresponding Google product record, or null if not found.</returns>
    Task<GoogleProductRecord> GetByIdAsync(int googleProductRecordId);

    /// <summary>
    /// Retrieves a Google product record by its associated product ID.
    /// </summary>
    /// <param name="productId">The product ID associated with the Google product record.</param>
    /// <returns>The corresponding Google product record, or null if not found.</returns>
    Task<GoogleProductRecord> GetByProductIdAsync(int productId);

    /// <summary>
    /// Inserts a new Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to insert.</param>
    Task InsertGoogleProductRecordAsync(GoogleProductRecord googleProductRecord);

    /// <summary>
    /// Updates an existing Google product record.
    /// </summary>
    /// <param name="googleProductRecord">The Google product record to update.</param>
    Task UpdateGoogleProductRecordAsync(GoogleProductRecord googleProductRecord);

    /// <summary>
    /// Retrieves a list of taxonomy entries from the embedded taxonomy file.
    /// </summary>
    /// <returns>A list of taxonomy entries.</returns>
    Task<IList<string>> GetTaxonomyListAsync();
}
