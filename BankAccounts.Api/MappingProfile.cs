using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        CreateMap<CreateAccountDto, CreateAccount.Command>();
    }
}