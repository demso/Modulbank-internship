using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.Database;
using MediatR;

namespace BankAccounts.Api.Features.Shared;
/// <summary>
/// Базовый класс для обработчиков запросов со вспомогательными методами
/// </summary>
/// <typeparam name="TRequest">Тип запроса</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public abstract class BaseRequestHandler<TRequest, TResponse>
     : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Проверяет существует ли счет с определенным id и принадлежит ли он пользователю
    /// </summary>
    /// <param name="dbContext">База данных</param>
    /// <param name="accountId">Id счета</param>
    /// <param name="ownerId">Id владельца</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Возвращает Account с определенным id</returns>
    /// <exception cref="AccountNotFoundException">В случае, если счет не найден или не принадлежит пользователю</exception>
    protected async Task<Account> GetValidAccount(IBankAccountsDbContext dbContext, int accountId, Guid ownerId, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([accountId], cancellationToken);

        if (account == null || account.OwnerId != ownerId)
            throw new AccountNotFoundException(accountId);
        
        return account;
    }
}