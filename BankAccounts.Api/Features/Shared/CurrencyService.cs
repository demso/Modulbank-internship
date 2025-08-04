namespace BankAccounts.Api.Features.Shared;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Сервис конвертации валют.
/// </summary>
public static class CurrencyService
{ 
    /// <summary>
    /// Тип валюты
    /// </summary>
    public enum Currencies
    {
        Rub,
        Usd,
        Eur
    }
    //1 EUR = 90 RUB
    //1 USD = 80 RUB
    /// <summary>
    /// Конвертирует валюту.
    /// </summary>
    /// <param name="sum">Сумма перобразуемой валюты</param>
    /// <param name="from">Тип валюты из которой преобразуем</param>
    /// <param name="to">Тип валюты в которую преобразуем</param>
    /// <returns>decimal</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static decimal Convert(decimal sum, Currencies from, Currencies to)
    {
        if (from.Equals(to))
           return sum;

        return (from, to) switch
        {
           (Currencies.Rub, Currencies.Eur) => sum / 90m,
           (Currencies.Rub, Currencies.Usd) => sum / 80m,
           (Currencies.Usd, Currencies.Rub) => sum * 80m,
           (Currencies.Usd, Currencies.Eur) => sum * 80m / 90m,
           (Currencies.Eur, Currencies.Usd) => sum * 1.125m,
           (Currencies.Eur, Currencies.Rub) => sum * 90m,
           _ => throw new ArgumentOutOfRangeException(nameof(from), from, "from: " + from + " to: " + to)
        };
    }
}