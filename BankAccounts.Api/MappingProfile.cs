using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions.Commands;
using BankAccounts.Api.Features.Transactions.Dtos;
using Transaction = BankAccounts.Api.Features.Transactions.Transaction;

namespace BankAccounts.Api;

public class MappingProfile : Profile
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MappingProfile"/>,
    /// настраивая правила преобразования между <see cref="AccountCreateDto"/> и <see cref="Account"/>,
    /// а также между <see cref="Account"/> и <see cref="AccountDto"/>.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>();

        //CreateMap<CreateAccountDto, CreateAccount.Command>().ForAllMembers(cfg => cfg.DoNotAllowNull());
        CreateMap<CreateAccountDto, CreateAccount.Command>();


        CreateMap<Transaction, TransactionDto>();

        CreateMap<PerformTransactionDto, PerformTransaction.Command>();

        CreateMap<PerformTransferDto, PerformTransfer.Command>();
    }
}