using BankAccounts.Api.Common;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace BankAccounts.Api.Infrastructure.Repository
{
    /// <summary>
    /// Абстрактный репозиторий представляющий реализацию некоторых методов
    /// </summary>
    /// <param name="dbContext">Контекст</param>
    public abstract class AbstractRepository(IBankAccountsDbContext dbContext) : IBankAccountsServiceRepositoryAsync
    {
        private protected readonly IBankAccountsDbContext DbContext = dbContext;
        
        /// <summary>
        /// Сохранение изменение в бд
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Добавить в таблицу исходящих сообщений
        /// </summary>
        /// <param name="serviceEvent">Событие</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T">Тип события</typeparam>
        public async Task AddToOutboxAsync<T>(T serviceEvent, CancellationToken cancellationToken = default) where T : Event
        {
            OutboxPublishedEntity entity = new() { 
                EventType = Event.GetEventType(serviceEvent), 
                Message = JsonHelper.ToJson(serviceEvent), 
                EventId = serviceEvent.EventId,
                CausationId = serviceEvent.Meta.CausationId,
                CorrelationId = serviceEvent.Meta.CorrelationId,
                Created = serviceEvent.OccurredAt 
            };
            
            await DbContext.OutboxPublished.AddAsync(entity, cancellationToken);
            
            await SaveChangesAsync(cancellationToken);
        }
        
        /// <summary>
        /// Метод для начала транзакции
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ISimpleTransactionScope> BeginSerializableTransactionAsync(CancellationToken cancellationToken =  default)
        {
            DbConnection connection = DbContext.Database.GetDbConnection();
            bool wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                await connection.OpenAsync(cancellationToken);

            DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await DbContext.Database.UseTransactionAsync(transaction, cancellationToken);
    
            return new SimpleTransactionScope(transaction, DbContext, wasClosed);
        }
    }

    /// <summary>
    /// Интерфейс представляющий собой простую реализацию TransactionScope для упрощенного управления транзакциями
    /// </summary>
    public interface ISimpleTransactionScope : IAsyncDisposable
    {
        /// <summary>
        /// Выполняет транзакцию
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task CommitAsync(CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Класс, представляющий собой простую реализацию TransactionScope для упрощенного управления транзакциями
    /// </summary>
    public class SimpleTransactionScope : ISimpleTransactionScope
    {
        /// <summary>
        /// Транзакция базы данных
        /// </summary>
        public readonly DbTransaction Transaction;
        private readonly IBankAccountsDbContext _dbContext;
        private readonly bool _wasClosed;
        private bool _disposed;

        internal SimpleTransactionScope(DbTransaction transaction, IBankAccountsDbContext dbContext, bool wasClosed)
        {
            Transaction = transaction;
            _dbContext = dbContext;
            _wasClosed = wasClosed;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
        
            try
            {
                await _dbContext.Database.UseTransactionAsync(null);
                if (_wasClosed)
                {
                    DbConnection connection = _dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Open)
                        await connection.CloseAsync();
                }
            }
            finally
            {
                await Transaction.DisposeAsync();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.CommitAsync(cancellationToken);
        }
    }
}
