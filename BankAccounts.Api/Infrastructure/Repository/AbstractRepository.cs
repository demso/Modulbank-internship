using BankAccounts.Api.Common;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace BankAccounts.Api.Infrastructure.Repository
{
    public abstract class AbstractRepository(IBankAccountsDbContext dbContext) : IBankAccountsServiceRepositoryAsync
    {
        private protected readonly IBankAccountsDbContext DbContext = dbContext;
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddToOutboxAsync<T>(T serviceEvent, CancellationToken cancellationToken = default) where T : Event
        {
            OutboxPublishedEntity entity = new() { 
                EventType = Event.GetEventType(serviceEvent), 
                Message = JsonHelper.ToJson(serviceEvent), 
                EventId = serviceEvent.EventId,
                CausationId = serviceEvent.Metadata.CausationId,
                CorrelationId = serviceEvent.Metadata.CorrelationId,
                Created = serviceEvent.OccurredAt 
            };
            
            await DbContext.OutboxPublished.AddAsync(entity, cancellationToken);
            
            await SaveChangesAsync(cancellationToken);
        }
        
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

    public interface ISimpleTransactionScope : IAsyncDisposable
    {
        public ValueTask DisposeAsync();
        public Task CommitAsync(CancellationToken cancellationToken = default);
    }
    
    public class SimpleTransactionScope : ISimpleTransactionScope
    {
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

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
        
            try
            {
                await _dbContext.Database.UseTransactionAsync(null);
                if (_wasClosed)
                {
                    var connection = _dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Open)
                        await connection.CloseAsync();
                }
            }
            finally
            {
                await Transaction.DisposeAsync();
                _disposed = true;
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.CommitAsync(cancellationToken);
        }
    }
}
