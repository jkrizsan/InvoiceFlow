using InvoiceFlow.Application.Contracts;

namespace InvoiceFlow.Application.Abstractions;

public interface IInvoicePdfGenerator
{
    byte[] Generate(InvoiceResponse invoice);
}

