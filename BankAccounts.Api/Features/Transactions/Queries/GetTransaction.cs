using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Queries;

/// <summary>
/// Получение данных о тразакции
/// </summary>
public static class GetTransaction
{
    /// <summary>
    /// Запрос данных о транзакции
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="TransactionId">Id транзакции</param>
    public record Query(
        Guid OwnerId,
        Guid TransactionId
    ) : IRequest<TransactionDto>;

    /// <summary>
    /// Обработчик команды.
    /// </summary>>
    public class Handler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, IMapper mapper) : BaseRequestHandler<Query, TransactionDto>
    {
        /// <summary>
        /// Обрабатывает команду.
        /// Выбрасывает исключение, если транзакция не найдена.
        /// </summary>
        /// <exception cref="NotFoundException"></exception>
        public override async Task<TransactionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await transactionsRepository.GetByIdAsync(request.TransactionId, cancellationToken);

            if (transaction == null)
                throw new NotFoundException(nameof(Transaction), request.TransactionId);

            await GetValidAccount(accountsRepository, transaction.AccountId, request.OwnerId, cancellationToken);

            return mapper.Map<TransactionDto>(transaction);
        }
    }

    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
    public class QueryValidator : AbstractValidator<Query>
    {
        /// <summary>
        /// Создание валидатора и настройка правил
        /// </summary>
        public QueryValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.TransactionId).NotEmpty();
        }
    }
}