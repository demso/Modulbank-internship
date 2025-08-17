namespace BankAccounts.Api.Features.Shared.UserBlacklist
{
    /// <summary>
    /// 
    /// </summary>
    public class UserBlacklist : IUserBlacklistService
    {
        private readonly HashSet<Guid> _blacklist = [];

        /// <inheritdoc />
        public void AddToList(Guid userId)
        {
            _blacklist.Add(userId);
        }

        /// <inheritdoc />
        public void RemoveFromList(Guid userId)
        {
            _blacklist.Remove(userId);
        }

        /// <inheritdoc />
        public bool IsBlacklisted(Guid userId)
        {
            return _blacklist.Contains(userId);
        }
    }
}
