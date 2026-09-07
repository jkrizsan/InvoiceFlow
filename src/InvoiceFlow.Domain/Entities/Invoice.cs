using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Domain.Entities;

public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice()
    {
    }

    private Invoice(
        string ownerId,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string sellerName,
        string sellerBusinessId,
        string sellerVatNumber,
        string sellerAddress,
        string sellerCountryCode,
        string customerName,
        string customerEmail,
        string customerAddress,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber,
        VatTreatment vatTreatment,
        string? notes,
        DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        OwnerId = Required(ownerId, nameof(ownerId));
        IssueDate = issueDate;
        SetDueDate(dueDate);
        Currency = Required(currency, nameof(currency)).ToUpperInvariant();
        SellerName = Required(sellerName, nameof(sellerName));
        SellerBusinessId = Required(sellerBusinessId, nameof(sellerBusinessId));
        SellerVatNumber = Required(sellerVatNumber, nameof(sellerVatNumber));
        SellerAddress = Required(sellerAddress, nameof(sellerAddress));
        SellerCountryCode = Country(sellerCountryCode);
        SetCustomer(
            customerName,
            customerEmail,
            customerAddress,
            customerCountryCode,
            customerType,
            customerVatNumber);
        VatTreatment = vatTreatment;
        Notes = CleanOptional(notes);
        Status = InvoiceStatus.Draft;
        Version = Guid.NewGuid();
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;
    public string? Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string SellerName { get; private set; } = string.Empty;
    public string SellerBusinessId { get; private set; } = string.Empty;
    public string SellerVatNumber { get; private set; } = string.Empty;
    public string SellerAddress { get; private set; } = string.Empty;
    public string SellerCountryCode { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerAddress { get; private set; } = string.Empty;
    public string CustomerCountryCode { get; private set; } = string.Empty;
    public CustomerType CustomerType { get; private set; }
    public string? CustomerVatNumber { get; private set; }
    public VatTreatment VatTreatment { get; private set; }
    public decimal NetTotal { get; private set; }
    public decimal VatTotal { get; private set; }
    public decimal GrossTotal { get; private set; }
    public string? Notes { get; private set; }
    public Guid Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public static Invoice Create(
        string ownerId,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string sellerName,
        string sellerBusinessId,
        string sellerVatNumber,
        string sellerAddress,
        string sellerCountryCode,
        string customerName,
        string customerEmail,
        string customerAddress,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber,
        VatTreatment vatTreatment,
        string? notes,
        DateTimeOffset now) =>
        new(
            ownerId,
            issueDate,
            dueDate,
            currency,
            sellerName,
            sellerBusinessId,
            sellerVatNumber,
            sellerAddress,
            sellerCountryCode,
            customerName,
            customerEmail,
            customerAddress,
            customerCountryCode,
            customerType,
            customerVatNumber,
            vatTreatment,
            notes,
            now);

    public void AddLine(
        string description,
        decimal quantity,
        decimal unitPrice,
        VatCategory vatCategory,
        decimal vatRate)
    {
        EnsureDraft();
        _lines.Add(new InvoiceLine(description, quantity, unitPrice, vatCategory, vatRate));
        RecalculateTotals();
    }

    public void ReplaceDraft(
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string customerName,
        string customerEmail,
        string customerAddress,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber,
        VatTreatment vatTreatment,
        string? notes,
        DateTimeOffset now)
    {
        EnsureDraft();
        IssueDate = issueDate;
        SetDueDate(dueDate);
        Currency = Required(currency, nameof(currency)).ToUpperInvariant();
        SetCustomer(
            customerName,
            customerEmail,
            customerAddress,
            customerCountryCode,
            customerType,
            customerVatNumber);
        VatTreatment = vatTreatment;
        Notes = CleanOptional(notes);
        _lines.Clear();
        RecalculateTotals();
        Touch(now);
    }

    public void PatchDraft(DateOnly? dueDate, string? notes, bool clearNotes, DateTimeOffset now)
    {
        EnsureDraft();

        if (dueDate.HasValue)
        {
            SetDueDate(dueDate.Value);
        }

        if (clearNotes)
        {
            Notes = null;
        }
        else if (notes is not null)
        {
            Notes = CleanOptional(notes);
        }

        Touch(now);
    }

    public void Issue(string number, DateTimeOffset now)
    {
        EnsureDraft();
        if (_lines.Count == 0)
        {
            throw new DomainRuleException("An invoice must contain at least one line before it can be issued.");
        }

        Number = Required(number, nameof(number));
        Status = InvoiceStatus.Issued;
        Touch(now);
    }

    public void MarkPaid(DateTimeOffset now)
    {
        if (Status != InvoiceStatus.Issued)
        {
            throw new DomainRuleException("Only an issued invoice can be marked as paid.");
        }

        Status = InvoiceStatus.Paid;
        Touch(now);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            throw new DomainRuleException("A paid or already cancelled invoice cannot be cancelled.");
        }

        Status = InvoiceStatus.Cancelled;
        Touch(now);
    }

    private void SetCustomer(
        string name,
        string email,
        string address,
        string countryCode,
        CustomerType type,
        string? vatNumber)
    {
        CustomerName = Required(name, nameof(name));
        CustomerEmail = Required(email, nameof(email));
        CustomerAddress = Required(address, nameof(address));
        CustomerCountryCode = Country(countryCode);
        CustomerType = type;
        CustomerVatNumber = CleanOptional(vatNumber)?.ToUpperInvariant();
    }

    private void SetDueDate(DateOnly dueDate)
    {
        if (dueDate < IssueDate)
        {
            throw new DomainRuleException("Due date cannot be earlier than issue date.");
        }

        DueDate = dueDate;
    }

    private void RecalculateTotals()
    {
        NetTotal = _lines.Sum(line => line.NetAmount);
        VatTotal = _lines.Sum(line => line.VatAmount);
        GrossTotal = _lines.Sum(line => line.GrossAmount);
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainRuleException("Only draft invoices can be edited.");
        }
    }

    private void Touch(DateTimeOffset now)
    {
        Version = Guid.NewGuid();
        UpdatedAt = now;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainRuleException($"{name} is required.")
            : value.Trim();

    private static string Country(string value)
    {
        string result = Required(value, nameof(value)).ToUpperInvariant();
        return result.Length == 2
            ? result
            : throw new DomainRuleException("Country code must contain two letters.");
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
