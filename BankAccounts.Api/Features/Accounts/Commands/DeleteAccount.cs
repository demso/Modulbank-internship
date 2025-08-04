using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;
/// <summary>
/// Удаление счета
/// </summary>
public static class DeleteAccount
{
    /// <summary>
    /// Команда удаления счета
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="AccountId">Id счета</param>
    public record Command(
        Guid OwnerId,
        int AccountId
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
        /// <exception cref="Exception">В случае, если на счету еще есть деньги</exception>>
        public override async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            if (account.Balance > 0)
                throw new Exception("Невозможно удалить счет пока баланс больше 0.");

            dbDbContext.Accounts.Remove(account);
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
        }
    }
}