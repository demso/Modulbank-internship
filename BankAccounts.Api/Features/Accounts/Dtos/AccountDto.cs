using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BankAccounts.Api.Mapping;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record AccountDto(
    Guid AccountId,
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal Balance,
    decimal? InterestRate,
    DateTime OpenDate,
    DateTime? CloseDate
);