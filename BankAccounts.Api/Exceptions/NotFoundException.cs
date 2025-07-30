namespace BankAccounts.Api.Exceptions;

public class NotFoundException(string name, object key) : Exception($"Exception \"{name}\" ({key}) not found.");