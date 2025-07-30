﻿namespace BankAccounts.Api.Features.Transactions;

public class TransactionDto
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CounterpartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime DateTime { get; set; }
}