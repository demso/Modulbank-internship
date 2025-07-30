using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Mapping;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal? InterestRate
) : IMapWith<CreateAccount.Command>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateAccountDto, CreateAccount.Command>()
            .ForMember(createCommand => createCommand.OwnerId,
                opt => opt.MapFrom(createDto => createDto.OwnerId))
            .ForMember(createCommand => createCommand.AccountType,
                opt => opt.MapFrom(createDto => createDto.AccountType))
            .ForMember(createCommand => createCommand.Currency,
                opt => opt.MapFrom(createDto => createDto.Currency))
            .ForMember(createCommand => createCommand.InterestRate,
                opt => opt.MapFrom(createDto => createDto.InterestRate));
    }
}