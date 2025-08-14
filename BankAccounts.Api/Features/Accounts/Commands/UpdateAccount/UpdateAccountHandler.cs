using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands.UpdateAccount;


    /// <summary>
    /// Обработчик команды <see cref="UpdateAccountCommand"/>
    /// </summary>
    public class UpdateAccountHandler(IAccountsRepositoryAsync accountsRepository) : RequestHandlerBase<UpdateAccountCommand, Unit>
    {
        /// <summary>
        /// Обрабатывает команду.
        /// Выбрасывает исключение в случае, если на счету еще есть деньги.
        /// </summary>>
        /// <exception cref="Exception"></exception>>
        public override async Task<Unit> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            Account account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

            if (account.CloseDate == null && request.InterestRate.HasValue)
                account.InterestRate = request.InterestRate.Value;

            if (account.CloseDate == null && request.Close.HasValue && request.Close.Value)
            {
                if (account.Balance != 0)
                    throw new BadRequestException("Невозможно закрыть счет на котором есть деньги.");
                account.CloseDate = DateTime.UtcNow;
            }

            await accountsRepository.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }