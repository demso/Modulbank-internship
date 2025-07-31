using AutoMapper;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Mapping;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record GetAllAccountsForUserDto(Guid OwnerId);