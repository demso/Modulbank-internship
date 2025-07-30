using AutoMapper;

namespace BankAccounts.Api.Mapping;

public interface IMapWith<T>
{
    void Mapping(Profile profile);
}