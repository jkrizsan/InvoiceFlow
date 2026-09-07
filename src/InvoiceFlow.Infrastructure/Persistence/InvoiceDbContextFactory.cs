using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=invoiceflow;Username=invoiceflow;Password=invoiceflow";

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new InvoiceDbContext(options);
    }
}

