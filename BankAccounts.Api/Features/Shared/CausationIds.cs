using System.Security.Cryptography;
using System.Text;

namespace BankAccounts.Api.Features.Shared
{
    public static class CausationIds
    {
        public static Guid CreateAccount => CreateGuidFromString("CreateAccount");

        private static Guid CreateGuidFromString(string input)
        {
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            Guid result = new Guid(hash);
            return result;
        }
    }
}
