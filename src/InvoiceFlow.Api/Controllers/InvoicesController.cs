using System.Security.Claims;
using InvoiceFlow.Api.Auth;
using InvoiceFlow.Application.Contracts;
using InvoiceFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Produces("application/json")]
public sealed class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.ReadInvoices)]
    [ProducesResponseType<PagedResponse<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    public Task<PagedResponse<InvoiceSummaryResponse>> Search(
        [FromQuery] InvoiceQuery query,
        CancellationToken cancellationToken) =>
        invoiceService.SearchAsync(OwnerId, query, cancellationToken);

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [Authorize(Policy = AuthPolicies.ReadInvoices)]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        InvoiceResponse invoice = await invoiceService.GetAsync(id, OwnerId, cancellationToken);
        SetEtag(invoice.Version);
        return Ok(invoice);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteInvoices)]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        InvoiceResponse invoice = await invoiceService.CreateAsync(OwnerId, request, cancellationToken);
        SetEtag(invoice.Version);
        return CreatedAtRoute(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthPolicies.WriteInvoices)]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<InvoiceResponse>> Replace(
        Guid id,
        ReplaceInvoiceRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        InvoiceResponse invoice = await invoiceService.ReplaceAsync(
            id,
            OwnerId,
            ParseEtag(ifMatch),
            request,
            cancellationToken);

        SetEtag(invoice.Version);
        return Ok(invoice);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = AuthPolicies.WriteInvoices)]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<InvoiceResponse>> Patch(
        Guid id,
        PatchInvoiceRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        InvoiceResponse invoice = await invoiceService.PatchAsync(
            id,
            OwnerId,
            ParseEtag(ifMatch),
            request,
            cancellationToken);

        SetEtag(invoice.Version);
        return Ok(invoice);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthPolicies.WriteInvoices)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        await invoiceService.DeleteAsync(
            id,
            OwnerId,
            ParseEtag(ifMatch),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = AuthPolicies.ReadInvoices)]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    public async Task<IActionResult> DownloadPdf(
        Guid id,
        CancellationToken cancellationToken)
    {
        (byte[] content, string fileName) = await invoiceService.GetPdfAsync(
            id,
            OwnerId,
            cancellationToken);

        return File(content, "application/pdf", fileName);
    }

    private string OwnerId =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Authenticated user has no subject claim.");

    private void SetEtag(Guid version) => Response.Headers.ETag = $"\"{version:D}\"";

    private static Guid? ParseEtag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string candidate = value.Trim().Trim('"');
        return Guid.TryParse(candidate, out Guid version)
            ? version
            : throw new ArgumentException("If-Match must contain a valid invoice ETag.");
    }
}
