using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BankAccounts.Api.Features.Accounts;

public class AccountMappingProfile : Profile
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AccountMappingProfile"/>,
    /// настраивая правила преобразования между <see cref="AccountCreateDto"/> и <see cref="Account"/>,
    /// а также между <see cref="Account"/> и <see cref="AccountDto"/>.
    /// </summary>
    public AccountMappingProfile()
    {
        CreateMap<Account, AccountDto>()
            .ForMember(accountDto => accountDto.AccountId,
                opt => opt.MapFrom(account => account.AccountId))
            .ForMember(accountDto => accountDto.OwnerId,
                opt => opt.MapFrom(account => account.OwnerId))
            .ForMember(accountDto => accountDto.AccountType,
                opt => opt.MapFrom(account => account.AccountType))
            .ForMember(accountDto => accountDto.Currency,
                opt => opt.MapFrom(account => account.Currency))
            .ForMember(accountDto => accountDto.Balance,
                opt => opt.MapFrom(account => account.Balance))
            .ForMember(accountDto => accountDto.InterestRate,
                opt => opt.MapFrom(account => account.InterestRate))
            .ForMember(accountDto => accountDto.OpenDate,
                opt => opt.MapFrom(account => account.OpenDate))
            .ForMember(accountDto => accountDto.CloseDate,
                opt => opt.MapFrom(account => account.CloseDate));

        CreateMap<CreateAccountDto, CreateAccount.Command>()
            .ForMember(createCommand => createCommand.OwnerId,
                opt => opt.MapFrom(createDto => createDto.OwnerId))
            .ForMember(createCommand => createCommand.AccountType,
                opt => opt.MapFrom(createDto => createDto.AccountType))
            .ForMember(createCommand => createCommand.Currency,
                opt => opt.MapFrom(createDto => createDto.Currency))
            .ForMember(createCommand => createCommand.InterestRate,
                opt => opt.MapFrom(createDto => createDto.InterestRate));

        CreateMap<GetAllAccountsForUserDto, GetAllAccountsForUser.Query>()
            .ForMember(getAllCommand => getAllCommand.OwnerId,
                opt => opt.MapFrom(getAllDto => getAllDto.UserId));
    }
}