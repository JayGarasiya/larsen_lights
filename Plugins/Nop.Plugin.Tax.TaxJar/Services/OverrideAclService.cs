using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// ACL service
/// </summary>
public class OverrideAclService : AclService
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    #endregion

    #region Ctor
    public OverrideAclService(
        CatalogSettings catalogSettings,
        ICustomerService customerService,
        INopDataProvider dataProvider,
        IRepository<AclRecord> aclRecordRepository,
        IStaticCacheManager staticCacheManager,
        Lazy<IWorkContext> workContext,
        TaxJarSettings taxJarSettings) : base(
            catalogSettings,
            customerService,
            dataProvider,
            aclRecordRepository,
            staticCacheManager,
            workContext)
    {
        _taxJarSettings = taxJarSettings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Apply ACL to the passed query
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports the ACL</typeparam>
    /// <param name="query">Query to filter</param>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filtered query
    /// </returns>
    public override async Task<IQueryable<TEntity>> ApplyAcl<TEntity>(IQueryable<TEntity> query, Customer customer)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(customer);

        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

        var newcustomerRoleIds = new List<int>();
        if (_workContext.Value.OriginalCustomerIfImpersonated is not null && _taxJarSettings.HiddenCustomerRoleId > 0)
            newcustomerRoleIds.Add(_taxJarSettings.HiddenCustomerRoleId);

        newcustomerRoleIds.AddRange(customerRoleIds.AsEnumerable());

        return await ApplyAcl(query, newcustomerRoleIds.ToArray());
    }

    /// <summary>
    /// Authorize ACL permission
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports the ACL</typeparam>
    /// <param name="entity">Entity</param>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the rue - authorized; otherwise, false
    /// </returns>
    public override async Task<bool> AuthorizeAsync<TEntity>(TEntity entity, Customer customer)
    {
        if (entity == null || customer == null)
            return false;

        if (_catalogSettings.IgnoreAcl || !entity.SubjectToAcl)
            return true;

        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        if (_workContext.Value.OriginalCustomerIfImpersonated is not null && _taxJarSettings.HiddenCustomerRoleId > 0)
            customerRoles.Add(await _customerService.GetCustomerRoleByIdAsync(_taxJarSettings.HiddenCustomerRoleId));

        foreach (var role1 in customerRoles)
        {
            foreach (var role2Id in await GetCustomerRoleIdsWithAccessAsync(entity.Id, nameof(TEntity)))
            {
                // Yes, we have such permission
                if (role1.Id == role2Id)
                    return true;
            }
        }

        // No permission found
        return false;
    }
    #endregion
}
