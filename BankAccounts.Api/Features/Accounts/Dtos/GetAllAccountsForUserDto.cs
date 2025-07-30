using AutoMapper;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Mapping;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record GetAllAccountsForUserDto(Guid UserId) : IMapWith<GetAllAccountsForUser.Query>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<GetAllAccountsForUserDto, GetAllAccountsForUser.Query>()
            .ForMember(getAllCommand => getAllCommand.OwnerId,
                opt => opt.MapFrom(getAllDto => getAllDto.UserId));
    }
}