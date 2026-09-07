using System.ComponentModel.DataAnnotations;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Contracts;

public sealed class CustomerRequest
{
    [Required, StringLength(160)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; init; } = string.Empty;

    [Required, RegularExpression("^[A-Za-z]{2}$")]
    public string CountryCode { get; init; } = string.Empty;

    [EnumDataType(typeof(CustomerType))]
    public CustomerType Type { get; init; }

    [StringLength(40)]
    public string? VatNumber { get; init; }
}

public sealed class InvoiceLineRequest
{
    [Required, StringLength(300)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal Quantity { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal UnitPrice { get; init; }

    [EnumDataType(typeof(VatCategory))]
    public VatCategory VatCategory { get; init; } = VatCategory.Standard;
}

public sealed class CreateInvoiceRequest : IValidatableObject
{
    public DateOnly? IssueDate { get; init; }
    public DateOnly? DueDate { get; init; }

    [RegularExpression("^[A-Za-z]{3}$")]
    public string? Currency { get; init; }

    [Required]
    public CustomerRequest Customer { get; init; } = new();

    [Required, MinLength(1), MaxLength(100)]
    public List<InvoiceLineRequest> Lines { get; init; } = [];

    [StringLength(2000)]
    public string? Notes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IssueDate.HasValue && DueDate.HasValue && DueDate < IssueDate)
        {
            yield return new ValidationResult(
                "Due date cannot be earlier than issue date.",
                [nameof(DueDate)]);
        }

        if (Customer.Type == CustomerType.Business &&
            !string.IsNullOrWhiteSpace(Customer.VatNumber) &&
            Customer.VatNumber.Length < 4)
        {
            yield return new ValidationResult(
                "A VAT number must contain at least four characters.",
                [nameof(Customer)]);
        }
    }
}

public sealed class ReplaceInvoiceRequest : IValidatableObject
{
    public DateOnly? IssueDate { get; init; }
    public DateOnly? DueDate { get; init; }

    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = string.Empty;

    [Required]
    public CustomerRequest Customer { get; init; } = new();

    [Required, MinLength(1), MaxLength(100)]
    public List<InvoiceLineRequest> Lines { get; init; } = [];

    [StringLength(2000)]
    public string? Notes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IssueDate.HasValue && DueDate.HasValue && DueDate < IssueDate)
        {
            yield return new ValidationResult(
                "Due date cannot be earlier than issue date.",
                [nameof(DueDate)]);
        }
    }
}

public sealed class PatchInvoiceRequest : IValidatableObject
{
    public DateOnly? DueDate { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }

    public bool ClearNotes { get; init; }
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus? Status { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!DueDate.HasValue && Notes is null && !ClearNotes && !Status.HasValue)
        {
            yield return new ValidationResult("At least one field must be supplied.");
        }

        if (ClearNotes && Notes is not null)
        {
            yield return new ValidationResult(
                "Notes and clearNotes cannot be supplied together.",
                [nameof(Notes), nameof(ClearNotes)]);
        }
    }
}
