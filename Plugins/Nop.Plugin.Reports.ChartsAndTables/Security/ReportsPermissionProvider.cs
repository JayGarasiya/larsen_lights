using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Reports.ChartsAndTables.Security
{
    /// <summary>
    /// Standard permission provider
    /// </summary>
    public partial class ReportsPermissionProvider : IPermissionConfigManager
    {
        public const string ACCESS_ADMIN_ACCESS_REPORTS = "AccessAdminReports";

        /// <summary>
        /// Gets all permission configurations
        /// </summary>
        public IList<PermissionConfig> AllConfigs => new List<PermissionConfig> {

            new ("Access admin. Access reports",ACCESS_ADMIN_ACCESS_REPORTS, nameof(StandardPermission.Configuration), NopCustomerDefaults.AdministratorsRoleName),
        };
    }
}
