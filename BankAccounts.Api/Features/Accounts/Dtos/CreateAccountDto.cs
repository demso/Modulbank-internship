using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Mapping;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal? InterestRate
);