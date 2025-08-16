using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.Repository;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs;

/// <summary>
/// Фоновое задание Hangfire для начисления процентов по счетам.
/// </summary>
/// <param name="context"></param>
/// <param name="logger"></param>
/// <param name="cancellationToken"></param>
// ReSharper disable once ClassNeverInstantiated.Global Класс используется Hangfire.
public class AccrueInterestJob(IAccountsRepositoryAsync accountsRepository, IBankAccountsDbContext context, ILogger<AccrueInterestJob> logger, 
    CancellationToken cancellationToken = default) 
{
    /// <summary>
    /// Фоновая задача
    /// </summary>
    public async Task Job()
    {
        List<int> accountIds = await context.Accounts.Where(a =>
                a.Balance != 0
                && a.CloseDate == null
                && a.InterestRate != 0)
            .AsNoTracking()
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);

        await using TransactionScope transaction = await accountsRepository.BeginSerializableTransactionAsync(cancellationToken);
       // await using TransactionScope transaction = await accountsRepository.BeginSerializableTransactionAsync(cancellationToken);

        try
        {
            await using NpgsqlCommand command = new("SELECT * FROM accrue_interest(@account_id)", (NpgsqlConnection) context.Database.GetDbConnection());
            
            foreach (int accountId in accountIds)
            {
                command.Parameters.AddWithValue("account_id", accountId);
                using var reader = await command.ExecuteReaderAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT public.accrue_interest({0})", accountId);
            }
            
            await transaction.CommitAsync();

            logger.LogInformation("Начисление процентов по счетам успешно. Количество измененных балансов счетов: {Count}", accountIds.Count);
        }
        catch (Exception ex)
        {
            const string msg = "Ошибка при начислении процентов, отмена операции ";
            throw new AccrueInterestException(msg, ex); 
        }
    }
}