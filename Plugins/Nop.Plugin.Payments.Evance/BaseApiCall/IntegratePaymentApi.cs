using Newtonsoft.Json;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Evance.Models;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.Evance.BaseApiCall
{
    public static class IntegratePaymentApi
    {
        private static HttpRequestMessage GetUrlEncodedContentHttpRequestMessage(string url, HttpMethod method, string content)
        {
            if (method == null)
                method = HttpMethod.Get;

            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(content))
            {
                // Deserialize the JSON content to a dictionary
                var dictionaryContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                if (dictionaryContent != null)
                {
                    // Create form data content from dictionary
                    var formContent = new FormUrlEncodedContent(dictionaryContent);

                    // Set request content to form data
                    request.Content = formContent;
                }
            }

            return request;
        }

        public static async Task<(string, bool, string)> WebApiRequestAsync<TPropType>(string url, BaseRequestModel? content = null, HttpMethod? method = null)
        {
            var logger = EngineContext.Current.Resolve<ILogger>();
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    var contentData = JsonConvert.SerializeObject(content.Parameters);
                    var request = GetUrlEncodedContentHttpRequestMessage(url, method, contentData);
                    var json = JsonConvert.SerializeObject(content, Formatting.Indented);

                    if (content != null && content.Parameters.Count > 0)
                    {
                        var formContent = new FormUrlEncodedContent(content.Parameters);
                        request.Content = formContent;
                    }

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        await logger.InsertLogAsync(LogLevel.Error, result);
                        return (result, false, json);
                    }

                    result = await response.Content.ReadAsStringAsync();
                    return (result, true, json);
                }
            }
            catch (Exception ex)
            {
                await logger.InsertLogAsync(LogLevel.Error, ex.Message);
                return (result, false, string.Empty);
            }
        }
    }
}
