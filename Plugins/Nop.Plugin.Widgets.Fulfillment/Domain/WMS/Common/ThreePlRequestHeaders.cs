using RestSharp;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Common
{
    /// <summary>
    /// Represents a 3PL central request headers
    /// </summary>
    internal static class ThreePlRequestHeaders
    {
        /// <summary>
        /// Represents a 3PL central common request header
        /// </summary>
        public static void AddCommonHeaders(this RestRequest request, string accessToken)
        {
            request.AddHeader("Accept", "application/hal+json");
            request.AddHeader("Content-Type", "application/hal+json; charset=utf-8");
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("Accept-Encoding", "gzip,deflate,sdch");
            request.AddHeader("Accept-Language", "en-US,en;q=0.8");
            request.AddHeader("Cache-Control", "no-cache");
        }

        /// <summary>
        /// Represents a 3PL central authentication request header
        /// </summary>
        public static void AddAuthHeaders(this RestRequest request, string base64String)
        {
            request.AddHeader("Host", "secure-wms.com");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", base64String);
            request.AddHeader("Accept-Encoding", "gzip,deflate,sdch");
            request.AddHeader("Accept-Language", "en-US,en;q=0.8");
        }
    }
}
