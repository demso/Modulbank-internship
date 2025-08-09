using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;
using BankAccounts.Api.Features.Transactions.Dtos;

namespace BankAccounts.Api.Common;

/// <summary>
/// Профиль для сопоставления типов
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MappingProfile"/>,
    /// настраивая правила преобразования между <see cref="CreateAccountDto"/> и <see cref="Account"/>,
    /// а также между <see cref="Account"/> и <see cref="AccountDto"/>.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<CreateAccountDto, CreateAccountCommand>();

        CreateMap<Transaction, TransactionDto>();

        CreateMap<PerformTransactionDto, PerformTransactionCommand>();

        CreateMap<PerformTransferDto, PerformTransferCommand>();
    }
}