using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs;

/// <summary>
/// Фоновое задание Hangfire для начисления процентов по счетам.
/// </summary>
/// <param name="context"></param>
/// <param name="logger"></param>
/// <param name="cancellationToken"></param>
// ReSharper disable once ClassNeverInstantiated.Global Класс используется Hangfire.
public class AccrueInterestJob(IBankAccountsDbContext context, ILogger<AccrueInterestJob> logger, CancellationToken cancellationToken = default) 
    : IJob
{

    /// <inheritdoc />
    public async Task Job()
    {
        var accountIds = await context.Accounts.Where(a =>
                a.Balance != 0
                && a.CloseDate == null
                && a.InterestRate != 0)
            .AsNoTracking()
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);


        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            foreach (var accountId in accountIds) 
                await context.Database.ExecuteSqlRawAsync("CALL public.accrue_interest({0})", accountId);

            await transaction.CommitAsync();

            logger.LogInformation("Начисление процентов по счетам успешно. Количество измененных счетов: " + accountIds.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Ошибка при начислении процентов, отмена операции ");
            throw; 
        }
    }
}