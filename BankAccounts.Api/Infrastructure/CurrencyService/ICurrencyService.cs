namespace BankAccounts.Api.Infrastructure.CurrencyService
{
    /// <summary>
    /// Интерфейс для сервиса конвертации валют.
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Конвертирует валюту <see cref="Currencies"/>.
        /// </summary>
        /// <param name="sum">Сумма преобразуемой валюты</param>
        /// <param name="from">Тип валюты из которой преобразуем (<see cref="Currencies"/>)</param>
        /// <param name="to">Тип валюты в которую преобразуем (<see cref="Currencies"/>)</param>
        /// <returns>decimal</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public decimal Convert(decimal sum, Currencies from, Currencies to);
    }
}