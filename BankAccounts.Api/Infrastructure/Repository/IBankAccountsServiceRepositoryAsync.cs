using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.Repository
{
    /// <summary>
    /// Базовый интерфейс для репозиториев сервиса банковских счетов.
    /// </summary>
    public interface IBankAccountsServiceRepositoryAsync
    {
        /// <summary>
        /// Метод для сохранения изменений в базе данных асинхронно.
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Добавит событие в таблицу outbox_published для публикации позже
        /// </summary>
        /// <param name="serviceEvent">Событие</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T">Тип события</typeparam>
        /// <returns></returns>
        public Task AddToOutboxAsync<T>(T serviceEvent, CancellationToken cancellationToken = default) where T : Event;

        /// <summary>
        /// Метод для начала транзакции базы данных
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ISimpleTransactionScope>
            BeginSerializableTransactionAsync(CancellationToken cancellationToken = default);
    }
}