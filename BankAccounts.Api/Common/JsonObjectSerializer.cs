using System.Text.Json;

namespace BankAccounts.Api.Common
{
    public static class JsonObjectSerializer
    {
        public static JsonSerializerOptions options = Infrastructure.Extensions.ServiceCollectionExtensions.JsonOptions;

        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }

        public static T? FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
