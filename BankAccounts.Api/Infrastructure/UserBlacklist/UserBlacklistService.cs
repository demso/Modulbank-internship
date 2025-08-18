using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.UserBlacklist
{
    /// <summary>
    /// 
    /// </summary>
    public class UserBlacklistService(IBankAccountsDbContext dbContext) : IUserBlacklistService
    {
        /// <inheritdoc />
        public async Task<bool> AddToList(Guid userId)
        {
            // TryAdd атомарно добавляет элемент, если его ещё нет.
            int count = await dbContext.BlockedUsers.Where(u => u.UserId == userId).CountAsync();
            
            if (count > 0)
                return false;
            
            await dbContext.BlockedUsers.AddAsync(new UserEntity { UserId = userId });
            
            await dbContext.SaveChangesAsync(CancellationToken.None);
            
            return true;
            // Или просто: _blacklist[userId] = default; // indexer тоже атомарный для ConcurrentDictionary
        }

        /// <inheritdoc />
        public async Task<bool> RemoveFromList(Guid userId)
        {
            // TryRemove атомарно удаляет элемент, если он существует.
            List<UserEntity> list =  await dbContext.BlockedUsers.Where(u => u.UserId == userId).ToListAsync();
            
            if (list.Count == 0)
                return false;
            
            dbContext.BlockedUsers.Remove(list[0]);
            
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> IsBlacklisted(Guid userId)
        {
            // ContainsKey атомарно проверяет наличие ключа.
            List<UserEntity> list =  await dbContext.BlockedUsers.Where(u => u.UserId == userId).ToListAsync();
            if (list.Count == 0)
                return false;
            return true;
        }
    }
}
