using System.Security.Cryptography;
using System.Text;

namespace BankAccounts.Api.Features.Shared
{
    /// <summary>
    /// Id источников событий
    /// </summary>
    public static class CausationIds
    {
        /// <summary>
        /// Создание счета
        /// </summary>
        public static Guid CreateAccount => CreateGuidFromString("CreateAccount");
        /// <summary>
        /// Проведение транзакции
        /// </summary>
        public static Guid PerformTransaction => CreateGuidFromString("PerformTransaction");
        /// <summary>
        /// Проведение трансфера
        /// </summary>
        public static Guid PerformTransfer => CreateGuidFromString("PerformTransfer");
        /// <summary>
        /// Начисление процентов по счету
        /// </summary>
        public static Guid AccrueInterest => CreateGuidFromString("AccrueInterest");

        private static Guid CreateGuidFromString(string input)
        {
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            Guid result = new(hash);
            return result;
        }
    }
}
