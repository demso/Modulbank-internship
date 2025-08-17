namespace BankAccounts.Api.Features.Shared.UserBlacklist
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
        void AddToList(Guid userId);
        /// <summary>
        /// Убрать из списка
        /// </summary>
        /// <param name="userId"></param>
        void RemoveFromList(Guid userId);
        /// <summary>
        /// Проверить есть ли в списке
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>True - есть, false - нет</returns>
        bool IsBlacklisted(Guid userId);
    }
}
