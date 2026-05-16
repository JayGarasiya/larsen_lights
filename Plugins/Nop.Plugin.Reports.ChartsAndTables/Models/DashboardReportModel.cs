using Nop.Web.Framework.Models;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    public record DashboardReportModel : BaseNopModel
    {
        #region Properties

        public bool DashboardWeekChart { get; set; }

        public bool DashboardMonthChart { get; set; }

        public bool DashboardYearChart { get; set; }

        public bool DashboardMonthYearChart { get; set; }

        public bool DashboardBestSellerProductYearChart { get; set; }

        public bool DashboardBestSellerProductMonthChart { get; set; }

        public int DashboardBestSellerProductLastNMonth { get; set; }

        public bool DashboardBestSellerCategoryYearChart { get; set; }

        public bool DashboardBestSellerManufacturerYearChart { get; set; }

        public bool DashboardBestSellerVendorYearChart { get; set; }

        public bool ManageProducts { get; set; }

        public bool ManageCategories { get; set; }

        public bool ManageManufacturers { get; set; }

        public bool ManageVendors { get; set; }

        #endregion
    }
}
