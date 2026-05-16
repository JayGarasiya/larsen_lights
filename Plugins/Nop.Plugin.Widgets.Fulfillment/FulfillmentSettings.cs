using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.Fulfillment
{
    /// <summary>
    /// Represents a plugin settings
    /// </summary>
    public class Fulfillmen3PLtSettings : ISettings
    {
        public Fulfillmen3PLtSettings()
        {
            MultiPackageForCountries = new List<int>();
        }

        /// <summary>
        /// Gets or sets the Client Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the ThreePl Key
        /// </summary>
        public string ThreePlKey { get; set; }

        /// <summary>
        /// Gets or sets the user id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Customer Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Facility Id
        /// </summary>
        public string FacilityId { get; set; }

        /// <summary>
        /// Gets or sets the Send Order Hours Interval 
        /// </summary>
        public int SendOrderHoursInterval { get; set; }

        /// <summary>
        /// Gets or sets the multiple packages an order total amount over 
        /// </summary>
        public List<int> MultiPackageForCountries { get; set; }

        /// <summary>
        /// Gets or sets the multiple packages an order total amount over 
        /// </summary>
        public decimal MultiPackageOrderAmountOver { get; set; }

        /// <summary>
        /// Gets or sets the don't splic packages an order total amount less 
        /// </summary>
        public decimal NoSplitAmountLess { get; set; }

        /// <summary>
        /// Gets or sets the Access Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets an ExpireOn
        /// </summary>
        public DateTime? ExpireOn { get; set; }

        public bool ValidateCredentials()
        {
            if (string.IsNullOrEmpty(this.ClientId) || string.IsNullOrEmpty(this.ClientSecret) || string.IsNullOrEmpty(this.ThreePlKey) ||
                string.IsNullOrEmpty(this.UserId) || string.IsNullOrEmpty(this.CustomerId) || string.IsNullOrEmpty(this.FacilityId))
                return false;

            return true;
        }

        public bool ValidateAccessToken()
        {
            if (string.IsNullOrEmpty(this.AccessToken))
                return false;

            if(this.ExpireOn.HasValue)
            {
                var span = DateTime.UtcNow.Subtract(this.ExpireOn.Value);
                var minutes = (int)Math.Round(span.TotalMinutes);
                if (minutes < 50)
                    return true;
            }

            return false;
        }
    }
}
