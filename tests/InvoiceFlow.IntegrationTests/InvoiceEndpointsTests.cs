using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace InvoiceFlow.IntegrationTests;

public sealed class InvoiceEndpointsTests(InvoiceFlowApiFactory factory)
    : IClassFixture<InvoiceFlowApiFactory>
{
    [Fact]
    public async Task Invoice_can_be_created_issued_and_downloaded_as_pdf()
    {
        using HttpClient client = factory.CreateClient();
        var request = new
        {
            issueDate = "2026-09-02",
            dueDate = "2026-09-16",
            currency = "EUR",
            customer = new
            {
                name = "Integration Test Customer",
                email = "customer@example.com",
                address = "Test Street 1, Kuopio",
                countryCode = "FI",
                type = "Consumer"
            },
            lines = new[]
            {
                new
                {
                    description = "Backend development",
                    quantity = 8m,
                    unitPrice = 75m,
                    vatCategory = "Standard"
                }
            }
        };

        HttpResponseMessage created = await client.PostAsJsonAsync("/api/v1/invoices", request);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.ETag);
        string etag = created.Headers.ETag?.ToString()
            ?? throw new InvalidOperationException("Create response has no ETag.");
        using JsonDocument body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Guid invoiceId = body.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(753m, body.RootElement.GetProperty("grossTotal").GetDecimal());

        HttpResponseMessage missingPrecondition = await client.PatchAsJsonAsync(
            $"/api/v1/invoices/{invoiceId}",
            new { status = "Issued" });
        Assert.Equal((HttpStatusCode)428, missingPrecondition.StatusCode);

        using var issueRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/invoices/{invoiceId}")
        {
            Content = JsonContent.Create(new { status = "Issued" })
        };
        issueRequest.Headers.TryAddWithoutValidation("If-Match", etag);

        HttpResponseMessage issued = await client.SendAsync(issueRequest);

        Assert.Equal(HttpStatusCode.OK, issued.StatusCode);
        using JsonDocument issuedBody = JsonDocument.Parse(await issued.Content.ReadAsStringAsync());
        string number = issuedBody.RootElement.GetProperty("number").GetString()
            ?? throw new InvalidOperationException("Issued invoice has no number.");
        Assert.StartsWith("INV-20260902-", number);

        HttpResponseMessage pdf = await client.GetAsync($"/api/v1/invoices/{invoiceId}/pdf");
        byte[] bytes = await pdf.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType?.MediaType);
        Assert.True(bytes.Length >= 4);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
