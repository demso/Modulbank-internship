using BankAccounts.Api.Features.Accounts;

namespace BankAccounts.Api.Exceptions;

public class AccountNotFoundException(int accountId) 
    : NotFoundException(nameof(Account), accountId, "У вас нет такого счета.");