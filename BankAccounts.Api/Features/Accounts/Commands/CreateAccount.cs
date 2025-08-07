using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

/// <summary>
/// Создание счета
/// </summary>
public static class CreateAccount
{
    /// <summary>
    /// Команда для создания счета
    /// </summary>
    public record Command : IRequest<AccountDto>
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        public Guid OwnerId { get; set; }
        /// <summary>
        /// Тип счета
        /// </summary>
        public AccountType AccountType { get; init; }
        /// <summary>
        /// Валюта
        /// </summary>
        public Currencies Currency { get; init; }
        /// <summary>
        /// Процентная ставка
        /// </summary>
        public decimal InterestRate { get; init; }
    }

    /// <summary>
    /// Обработчик команды
    /// </summary>
    public class Handler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : BaseRequestHandler<Command, AccountDto>
    {
        /// <inheritdoc />
        public override async Task<AccountDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await accountsRepository.AddAsync(request.OwnerId, request.AccountType, request.Currency, 
                    request.InterestRate, cancellationToken);

            await accountsRepository.SaveChangesAsync(cancellationToken);

            return mapper.Map<AccountDto>(account);
        }
    }
    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
    public class CommandValidator : AbstractValidator<Command>
    {
        /// <summary>
        /// Создание валидатора и настройка правил
        /// </summary>
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
        }
    }
}