using System.Text.Json;

namespace BankAccounts.Api.Common
{
    /// <summary>
    /// Имеет методы для сериализации/десериализации объектов
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Настройки JsonSerializer
        /// </summary>
        public static readonly JsonSerializerOptions Options = new();

        /// <summary>
        /// В json текст
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
        /// <summary>
        /// Из json-текста
        /// </summary>
        /// <param name="json">Json-текст для десериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Десериализованный объект</returns>
        public static T? FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
