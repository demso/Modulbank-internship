using BankAccounts.Api.Features.Transactions.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;

/// <summary>
/// Команда для проведения трансфера 
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global Класс используется посредником
public record PerformTransferCommand : IRequest<TransactionDto>
{
    /// <summary>
    /// Id пользователя
    /// </summary>
    public Guid OwnerId { get; set; }
    /// <summary>
    /// Id исходящего счета
    /// </summary>
    public int FromAccountId { get; init; }
    /// <summary>
    /// Id счета назначения
    /// </summary>
    public int ToAccountId { get; init; }
    /// <summary>
    /// Сумма денежных средств
    /// </summary>
    public decimal Amount { get; init; }
}