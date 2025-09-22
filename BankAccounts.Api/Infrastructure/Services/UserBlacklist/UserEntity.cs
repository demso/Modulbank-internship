namespace BankAccounts.Api.Infrastructure.UserBlacklist
{
    /// <summary>
    /// Сущность пользователя для работы с черным списком (блокировкой)
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// Идентификатор строки в таблице
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; init; }
    }
}
