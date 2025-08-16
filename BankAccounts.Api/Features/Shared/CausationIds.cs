using System.Security.Cryptography;
using System.Text;

namespace BankAccounts.Api.Features.Shared
{
    public static class CausationIds
    {
        public static Guid CreateAccount => CreateGuidFromString("CreateAccount");
        public static Guid PerformTransaction => CreateGuidFromString("PerformTransaction");
        public static Guid PerformTransfer => CreateGuidFromString("PerformTransfer");
        public static Guid AccrueInterest => CreateGuidFromString("AccrueInterest");

        private static Guid CreateGuidFromString(string input)
        {
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            Guid result = new(hash);
            return result;
        }
    }
}
