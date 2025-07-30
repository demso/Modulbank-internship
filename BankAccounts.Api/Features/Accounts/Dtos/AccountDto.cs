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
) : IMapWith<Account>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Account, AccountDto>()
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
    }
}