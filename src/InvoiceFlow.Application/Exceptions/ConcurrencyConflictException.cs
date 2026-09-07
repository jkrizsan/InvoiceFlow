namespace InvoiceFlow.Application.Exceptions;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

