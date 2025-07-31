namespace BankAccounts.Api.Features;

public static class CurrencyService
{
   public enum Currencies
   {
       RUB,
       USD,
       EUR
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
       switch ((from, to))
       {
           case (Currencies.RUB, Currencies.EUR):
               return sum / 90m;
           case (Currencies.RUB, Currencies.USD):
               return sum / 80m;

           case (Currencies.USD, Currencies.RUB):
               return sum * 80m;
           case (Currencies.USD, Currencies.EUR):
               return sum * 80m/90m;

           case (Currencies.EUR, Currencies.USD):
               return sum * 1.125m;
           case (Currencies.EUR, Currencies.RUB):
               return sum * 90m;

            default:
               throw new ArgumentOutOfRangeException(nameof(from), from, null);
       }

   }
}