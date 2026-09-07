namespace InvoiceFlow.Application.Contracts;

public sealed record SellerProfile(
    string Name,
    string BusinessId,
    string VatNumber,
    string Address,
    string CountryCode,
    string DefaultCurrency);

