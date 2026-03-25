namespace CaManagement.Api.Services.Exceptions;

public sealed class DomainValidationException(string message) : Exception(message);
