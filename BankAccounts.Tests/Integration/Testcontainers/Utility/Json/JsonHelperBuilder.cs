using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankAccounts.Tests.Integration.Testcontainers.Utility.Json
{
    public class JsonHelperBuilder 
    {
        public static JsonHelper BuildHelper()
        {
            JsonHelper helper = new();
            helper.Options.Converters.Add(new JsonStringEnumConverter());
            helper.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            helper.Options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            return helper;
        }
    }
}
