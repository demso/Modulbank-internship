using BankAccounts.Api.Features.Accounts;

namespace BankAccounts.Api.Exceptions;

/// <summary>
/// Исключение, возвращаемое в случае, если счет с определенным id не найден.
/// </summary>
/// <param name="accountId"></param>
public class AccountNotFoundException(int accountId) 
    : NotFoundException(nameof(Account), accountId, "У вас нет такого счета.");