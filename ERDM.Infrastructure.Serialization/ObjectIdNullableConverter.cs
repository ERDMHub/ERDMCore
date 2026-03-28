using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ERDM.Infrastructure.Serialization
{
    public class ObjectIdNullableConverter : JsonConverter<ObjectId?>
    {
        public override ObjectId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? null : new ObjectId(value);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}
