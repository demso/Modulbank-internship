using BankAccounts.Api.Features.Transactions.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransaction;

/// <summary>
/// Запрос данных о транзакции
/// </summary>
/// <param name="OwnerId">Id владельца</param>
/// <param name="TransactionId">Id транзакции</param>
public record GetTransactionQuery(
    Guid OwnerId,
    Guid TransactionId
) : IRequest<TransactionDto>;