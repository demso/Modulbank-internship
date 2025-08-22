using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Features.Accounts.Dtos;

namespace BankAccounts.Api.Features.Accounts;

/// <summary>
/// Профиль для сопоставления типов
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется mapper
// ReSharper disable once UnusedMember.Global
public class AccountsMappingProfile : Profile 
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AccountsMappingProfile"/>,
    /// настраивая правила преобразования между <see cref="CreateAccountDto"/> и <see cref="CreateAccountCommand"/>,
    /// а также между <see cref="Account"/> и <see cref="AccountDto"/>.
    /// </summary>
    public AccountsMappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<CreateAccountDto, CreateAccountCommand>();
    }
}