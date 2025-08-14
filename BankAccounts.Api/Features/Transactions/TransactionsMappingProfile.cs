using AutoMapper;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;
using BankAccounts.Api.Features.Transactions.Dtos;

namespace BankAccounts.Api.Features.Transactions;

/// <summary>
/// Профиль для сопоставления типов для работы с транзакциями.
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется маппером
// ReSharper disable once UnusedMember.Global
public class TransactionsMappingProfile : Profile
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="TransactionsMappingProfile"/>,
    /// настраивая правила преобразования между <see cref="Transaction"/> и <see cref="TransactionDto"/>,
    /// между <see cref="PerformTransactionDto"/> и <see cref="PerformTransactionCommand"/>
    /// и <see cref="PerformTransferDto"/> и <see cref="PerformTransferCommand"/>
    /// </summary>
    public TransactionsMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>();

        CreateMap<PerformTransactionDto, PerformTransactionCommand>();

        CreateMap<PerformTransferDto, PerformTransferCommand>();
    }
}
