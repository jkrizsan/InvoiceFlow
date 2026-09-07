using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Domain.Entities;

public sealed class InvoiceLine
{
    private InvoiceLine()
    {
    }

    internal InvoiceLine(
        string description,
        decimal quantity,
        decimal unitPrice,
        VatCategory vatCategory,
        decimal vatRate)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainRuleException("Line description is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainRuleException("Line quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainRuleException("Unit price cannot be negative.");
        }

        if (vatRate is < 0 or > 100)
        {
            throw new DomainRuleException("VAT rate must be between 0 and 100.");
        }

        Id = Guid.NewGuid();
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = RoundMoney(unitPrice);
        VatCategory = vatCategory;
        VatRate = vatRate;
        NetAmount = RoundMoney(quantity * unitPrice);
        VatAmount = RoundMoney(NetAmount * vatRate / 100m);
        GrossAmount = NetAmount + VatAmount;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public VatCategory VatCategory { get; private set; }
    public decimal VatRate { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal VatAmount { get; private set; }
    public decimal GrossAmount { get; private set; }

    private static decimal RoundMoney(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}

