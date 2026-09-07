namespace InvoiceFlow.Application.Exceptions;

public sealed class VatRateNotFoundException(string message) : Exception(message);

