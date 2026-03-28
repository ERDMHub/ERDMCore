using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERDM.Infrastructure.Serialization
{
    public static class SerializationExtensions
    {
        public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
            });
        }

        public static T DeepClone<T>(this T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json)
                ?? throw new InvalidOperationException("Deserialization returned null during deep clone.");
        }
    }
}