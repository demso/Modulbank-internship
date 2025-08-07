using BankAccounts.Api.Features.Transactions.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;

/// <summary>
/// Команда для проведения транзакции
/// </summary>
public record PerformTransactionCommand : IRequest<TransactionDto>
{
    /// <summary>
    /// Id владельца счета
    /// </summary>
    public Guid OwnerId { get; set; }
    /// <summary>
    /// Id счета
    /// </summary>
    public int AccountId { get; set; }
    /// <summary>
    /// Тип транзакции
    /// </summary>
    public TransactionType TransactionType { get; init; }
    /// <summary>
    /// Сумма денежных средств
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string? Description { get; init; }
}