using InvoiceFlow.Application.Services;
using InvoiceFlow.Domain.Entities;

namespace InvoiceFlow.UnitTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_has_no_dependency_on_outer_layers()
    {
        string[] references = typeof(Invoice).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("InvoiceFlow.Application", references);
        Assert.DoesNotContain("InvoiceFlow.Infrastructure", references);
        Assert.DoesNotContain("InvoiceFlow.Api", references);
    }

    [Fact]
    public void Application_has_no_dependency_on_infrastructure_or_api()
    {
        string[] references = typeof(InvoiceService).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("InvoiceFlow.Infrastructure", references);
        Assert.DoesNotContain("InvoiceFlow.Api", references);
    }
}
