using Newtonsoft.Json;
using Nop.Plugin.Widgets.MakeTypeModel.Models.HyCapacityAPI;
using System.Net.Http.Headers;

namespace Nop.Plugin.Widgets.MakeTypeModel.BaseApiCall
{
    /// <summary>
    /// Represents a hycapacity API
    /// </summary>
    public static class HyCapacityAPI
    {
        #region Methods

        /// <summary>
        /// Get authorization token
        /// </summary>
        /// <param name="authUrl">auth Url</param>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <returns>
        /// A task that represents the asynchronous operation 
        /// </returns>
        public static async Task<string> GetAuthorizationTokenAsync(string authUrl, string username, string password)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = await client.PostAsync(authUrl, formData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Token request failed: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<AuthAPIResponseModel>(json);

            return tokenResponse?.access_token;
        }

        /// <summary>
        /// Inventory Api
        /// </summary>
        /// <param name="authorizationToken">authorization Token</param>
        /// <param name="tokenGuidId">token guid identifier</param>
        /// <param name="apiUrl">api Url</param>
        /// <returns>
        /// A task that represents the asynchronous operation 
        /// </returns>
        public static async Task<string> InventoryAPIAsync(string authorizationToken, string tokenGuidId, string apiUrl)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                tokenGuidId = tokenGuidId,
            };

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Inventory API request failed: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            return responseJson ?? string.Empty;
        }

        #endregion
    }
}
