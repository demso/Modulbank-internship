namespace BankAccounts.Api.Features.CurrencyService;
/// <summary>
/// Сервис конвертации валют.
/// </summary>
public class CurrencyService : ICurrencyService
{ 
    //1 EUR = 90 RUB
    //1 USD = 80 RUB
    /// <inheritdoc />
    public decimal Convert(decimal sum, Currencies from, Currencies to)
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