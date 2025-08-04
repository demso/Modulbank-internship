#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BankAccounts.Api.Common.Exceptions;

public class NotFoundException(string name, object key, string details = "") : Exception($"Exception \"{name}\" ({key}) not found. " + details);