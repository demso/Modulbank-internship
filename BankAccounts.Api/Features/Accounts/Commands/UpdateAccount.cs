using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

/// <summary>
/// Изменение свойств счета
/// </summary>
public static class UpdateAccount
{
    /// <summary>
    /// Команда для закрытия счета
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="AccountId">Id счета</param>
    /// <param name="InterestRate">Процентная ставка</param>
    /// <param name="Close">Нужно ли закрыть счет</param>
    public record Command(
        Guid OwnerId,
        int AccountId,
        decimal? InterestRate,
        bool? Close
    ) : IRequest<Unit>;

    /// <summary>
    /// Обработчик команды
    /// </summary>
    public class Handler(IBankAccountsDbContext dbDbContext) : BaseRequestHandler<Command, Unit>
    {
        /// <summary>
        /// Обрабатывает команду.
        /// Выбрасывает исключение в случае, если на счету еще есть деньги.
        /// </summary>>
        /// <exception cref="Exception"></exception>>
        public override async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            if (account.CloseDate == null && request.InterestRate.HasValue)
                account.InterestRate = request.InterestRate.Value;

            if (account.CloseDate == null && request.Close.HasValue && request.Close.Value)
            {
                if (account.Balance != 0)
                    throw new BadRequestException("Невозможно закрыть счет на котором есть деньги.");
                account.CloseDate = DateTime.Now;
            }

            dbDbContext.Accounts.Update(account);
            await dbDbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
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
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
        }
    }
}


