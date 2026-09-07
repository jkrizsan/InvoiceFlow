namespace InvoiceFlow.Domain;

public sealed class DomainRuleException(string message) : Exception(message);

