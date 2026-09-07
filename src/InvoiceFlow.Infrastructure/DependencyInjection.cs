using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Contracts;
using InvoiceFlow.Application.Services;
using InvoiceFlow.Infrastructure.Pdf;
using InvoiceFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace InvoiceFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInvoiceFlowInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is missing.");

        services.AddDbContext<InvoiceDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(InvoiceDbContext).Assembly.GetName().Name!)));

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IVatRateRepository, VatRateRepository>();
        services.AddScoped<IVatCalculator, VatCalculator>();
        services.AddScoped<IInvoicePdfGenerator, QuestInvoicePdfGenerator>();
        services.AddScoped<InvoiceService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(CreateSellerProfile(configuration));

        QuestPDF.Settings.License = LicenseType.Community;
        return services;
    }

    private static SellerProfile CreateSellerProfile(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Seller");
        return new SellerProfile(
            Required(section["Name"], "Seller:Name"),
            Required(section["BusinessId"], "Seller:BusinessId"),
            Required(section["VatNumber"], "Seller:VatNumber"),
            Required(section["Address"], "Seller:Address"),
            Required(section["CountryCode"], "Seller:CountryCode").ToUpperInvariant(),
            Required(section["DefaultCurrency"], "Seller:DefaultCurrency").ToUpperInvariant());
    }

    private static string Required(string? value, string key) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Configuration value '{key}' is missing.")
            : value;
}
