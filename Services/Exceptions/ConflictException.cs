namespace CaManagement.Api.Services.Exceptions;

/// <summary>Thrown when a request conflicts with the current state of a resource (HTTP 409).</summary>
public sealed class ConflictException(string message) : Exception(message);
