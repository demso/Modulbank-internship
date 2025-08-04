using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Commands;
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

        CreateMap<CreateAccountDto, CreateAccount.Command>();

        CreateMap<Transaction, TransactionDto>();

        CreateMap<PerformTransactionDto, PerformTransaction.Command>();

        CreateMap<PerformTransferDto, PerformTransfer.Command>();
    }
}