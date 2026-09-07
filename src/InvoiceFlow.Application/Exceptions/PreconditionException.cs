namespace InvoiceFlow.Application.Exceptions;

public sealed class PreconditionRequiredException(string message) : Exception(message);

public sealed class PreconditionFailedException(string message) : Exception(message);

