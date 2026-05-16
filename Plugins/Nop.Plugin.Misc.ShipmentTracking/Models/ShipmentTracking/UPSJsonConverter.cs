using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents an UPS json converter
    /// </summary>
    public class UPSJsonConverter<T> : JsonConverter where T : class
    {
        public override bool CanConvert(Type objectType) { return true; }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = new object();
            if (reader.TokenType == JsonToken.StartObject)
                obj = new List<T>() { (T)serializer.Deserialize(reader, typeof(T)) };
            else if (reader.TokenType == JsonToken.StartArray)
                obj = serializer.Deserialize(reader, objectType);
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}
