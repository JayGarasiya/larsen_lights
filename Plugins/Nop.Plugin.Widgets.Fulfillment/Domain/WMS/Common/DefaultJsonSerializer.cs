using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Common
{
    /// <summary>
    /// Specifies the settings on a <see cref="JsonSerializer"/> object.
    /// </summary>
    internal class DefaultJsonSerializer : JsonSerializerSettings
    {
        public DefaultJsonSerializer()
        {
            NullValueHandling = NullValueHandling.Ignore;
            DefaultValueHandling = DefaultValueHandling.Ignore;
            ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
