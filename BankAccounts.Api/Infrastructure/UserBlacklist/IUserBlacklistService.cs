namespace BankAccounts.Api.Infrastructure.UserBlacklist
{
    /// <summary>
    /// Интерфейс для создания черного списка пользователей
    /// </summary>
    public interface IUserBlacklistService
    {
        /// <summary>
        /// Добавить в список
        /// </summary>
        /// <param name="userId"></param>
        Task<bool> AddToList(Guid userId);
        /// <summary>
        /// Убрать из списка
        /// </summary>
        /// <param name="userId"></param>
        Task<bool> RemoveFromList(Guid userId);
        /// <summary>
        /// Проверить есть ли в списке
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>True - есть, false - нет</returns>
        Task<bool> IsBlacklisted(Guid userId);
    }
}
