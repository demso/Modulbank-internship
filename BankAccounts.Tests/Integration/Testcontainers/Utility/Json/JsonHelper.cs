using System.Text.Json;

namespace BankAccounts.Tests.Integration.Testcontainers.Utility.Json
{
    public class JsonHelper : IJsonHelper
    {
        public readonly JsonSerializerOptions Options = new();

        /// <summary>
        /// В json текст
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <returns></returns>
        public string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
        /// <summary>
        /// Из json-текста
        /// </summary>
        /// <param name="json">Json-текст для десериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Десериализованный объект</returns>
        // ReSharper disable once UnusedMember.Global Оставлен на будущее 
        public T? FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
