namespace BankAccounts.Tests.Integration.Testcontainers.Utility.Json
{
    public interface IJsonHelper
    {
        string ToJson(object obj);
        T? FromJson<T>(string json);
    }
}
