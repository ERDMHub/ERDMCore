using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERDM.Infrastructure.Serialization
{
    public static class JsonSerializer
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public static string Serialize<T>(T obj, JsonSerializerOptions options = null)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }

        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions(DefaultOptions);
        }
    }
}
