using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using MediatR;

namespace BankAccounts.Api.Features.Shared;
/// <summary>
/// Базовый класс для обработчиков запросов со вспомогательными методами
/// </summary>
/// <typeparam name="TRequest">Тип запроса</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public abstract class RequestHandlerBase<TRequest, TResponse>
     : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Проверяет, существует ли счет (<see cref="Account"/>) с определенным id и принадлежит ли он пользователю
    /// </summary>
    /// <param name="accountsRepository">Репозиторий банковских счетов <see cref="AccountsRepositoryAsync"/></param>
    /// <param name="accountId">Id счета</param>
    /// <param name="ownerId">Id владельца</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Возвращает <see cref="Account"/> с определенным id</returns>
    /// <exception cref="AccountNotFoundException">В случае, если счет не найден или не принадлежит пользователю</exception>
    protected async Task<Account> GetValidAccount(IAccountsRepositoryAsync accountsRepository, int accountId, 
        Guid ownerId, CancellationToken cancellationToken)
    {
        var account = await accountsRepository.GetByIdAsync(accountId, cancellationToken);

        if (account == null || account.OwnerId != ownerId)
            throw new AccountNotFoundException(accountId);
        
        return account;
    }
}