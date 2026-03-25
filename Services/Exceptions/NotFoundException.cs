namespace CaManagement.Api.Services.Exceptions;

public sealed class NotFoundException(string message) : Exception(message);
