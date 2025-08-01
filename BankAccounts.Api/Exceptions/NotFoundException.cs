namespace BankAccounts.Api.Exceptions;

public class NotFoundException(string name, object key, string details = "") : Exception($"Exception \"{name}\" ({key}) not found. " + details);