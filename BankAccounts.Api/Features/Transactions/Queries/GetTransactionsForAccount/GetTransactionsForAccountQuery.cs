using BankAccounts.Api.Features.Transactions.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;

/// <summary>
/// Запрос выписки
/// </summary>
/// <param name="OwnerId">Id владельца</param>
/// <param name="AccountId">Id счета</param>
/// <param name="FromDate">Начало периода (может быть null)</param>
/// <param name="ToDate">Конец периода (может быть null)</param>
public record GetTransactionsForAccountQuery(
    Guid OwnerId,
    int AccountId,
    DateOnly? FromDate,
    DateOnly? ToDate
) : IRequest<List<TransactionDto>>;