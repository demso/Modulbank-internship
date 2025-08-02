namespace BankAccounts.Api.Features;

public static class CurrencyService
{
   public enum Currencies
   {
       Rub,
       Usd,
       Eur
   }
    //1 EUR = 90 RUB
    //1 USD = 80 RUB
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sum"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
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