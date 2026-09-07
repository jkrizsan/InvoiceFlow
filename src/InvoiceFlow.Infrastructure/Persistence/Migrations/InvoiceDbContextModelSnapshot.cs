using InvoiceFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InvoiceFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InvoiceDbContext))]
public sealed class InvoiceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("InvoiceFlow.Domain.Entities.Invoice", entity =>
        {
            entity.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            entity.Property<string>("Currency")
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("character varying(3)");

            entity.Property<string>("CustomerAddress")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("character varying(300)");

            entity.Property<string>("CustomerCountryCode")
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(2)
                .HasColumnType("character(2)");

            entity.Property<string>("CustomerEmail")
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnType("character varying(254)");

            entity.Property<string>("CustomerName")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("character varying(160)");

            entity.Property<int>("CustomerType")
                .HasColumnType("integer");

            entity.Property<string>("CustomerVatNumber")
                .HasMaxLength(40)
                .HasColumnType("character varying(40)");

            entity.Property<DateOnly>("DueDate")
                .HasColumnType("date");

            entity.Property<decimal>("GrossTotal")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<DateOnly>("IssueDate")
                .HasColumnType("date");

            entity.Property<decimal>("NetTotal")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<string>("Notes")
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            entity.Property<string>("Number")
                .HasMaxLength(40)
                .HasColumnType("character varying(40)");

            entity.Property<string>("OwnerId")
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnType("character varying(128)");

            entity.Property<string>("SellerAddress")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("character varying(300)");

            entity.Property<string>("SellerBusinessId")
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnType("character varying(40)");

            entity.Property<string>("SellerCountryCode")
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(2)
                .HasColumnType("character(2)");

            entity.Property<string>("SellerName")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("character varying(160)");

            entity.Property<string>("SellerVatNumber")
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnType("character varying(40)");

            entity.Property<int>("Status")
                .HasColumnType("integer");

            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            entity.Property<decimal>("VatTotal")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<int>("VatTreatment")
                .HasColumnType("integer");

            entity.Property<Guid>("Version")
                .IsConcurrencyToken()
                .HasColumnType("uuid");

            entity.HasKey("Id");

            entity.HasIndex("Number")
                .IsUnique()
                .HasFilter("\"Number\" IS NOT NULL");

            entity.HasIndex("OwnerId", "Status");

            entity.ToTable("Invoices");
        });

        modelBuilder.Entity("InvoiceFlow.Domain.Entities.InvoiceLine", entity =>
        {
            entity.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            entity.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("character varying(300)");

            entity.Property<decimal>("GrossAmount")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<Guid>("InvoiceId")
                .HasColumnType("uuid");

            entity.Property<decimal>("NetAmount")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<decimal>("Quantity")
                .HasPrecision(18, 3)
                .HasColumnType("numeric(18,3)");

            entity.Property<decimal>("UnitPrice")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<decimal>("VatAmount")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            entity.Property<int>("VatCategory")
                .HasColumnType("integer");

            entity.Property<decimal>("VatRate")
                .HasPrecision(5, 2)
                .HasColumnType("numeric(5,2)");

            entity.HasKey("Id");
            entity.HasIndex("InvoiceId");
            entity.ToTable("InvoiceLines");
        });

        modelBuilder.Entity("InvoiceFlow.Domain.Entities.VatRate", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(entity.Property<int>("Id"));

            entity.Property<int>("Category")
                .HasColumnType("integer");

            entity.Property<string>("CountryCode")
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(2)
                .HasColumnType("character(2)");

            entity.Property<decimal>("Rate")
                .HasPrecision(5, 2)
                .HasColumnType("numeric(5,2)");

            entity.Property<DateOnly>("ValidFrom")
                .HasColumnType("date");

            entity.Property<DateOnly?>("ValidTo")
                .HasColumnType("date");

            entity.HasKey("Id");
            entity.HasIndex("CountryCode", "Category", "ValidFrom").IsUnique();
            entity.ToTable("VatRates");

            entity.HasData(CreateVatRateSeed());
        });

        modelBuilder.Entity("InvoiceFlow.Domain.Entities.InvoiceLine", entity =>
        {
            entity.HasOne("InvoiceFlow.Domain.Entities.Invoice", null)
                .WithMany("Lines")
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("InvoiceFlow.Domain.Entities.Invoice", entity =>
        {
            entity.Navigation("Lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
#pragma warning restore 612, 618
    }

    private static object[] CreateVatRateSeed()
    {
        (string Country, decimal Rate)[] rates =
        [
            ("AT", 20m), ("BE", 21m), ("BG", 20m), ("HR", 25m),
            ("CY", 19m), ("CZ", 21m), ("DK", 25m), ("EE", 24m),
            ("FI", 25.5m), ("FR", 20m), ("DE", 19m), ("GR", 24m),
            ("HU", 27m), ("IE", 23m), ("IT", 22m), ("LV", 21m),
            ("LT", 21m), ("LU", 17m), ("MT", 18m), ("NL", 21m),
            ("PL", 23m), ("PT", 23m), ("RO", 21m), ("SK", 23m),
            ("SI", 22m), ("ES", 21m), ("SE", 25m)
        ];

        return rates.Select((entry, index) => (object)new
        {
            Id = -(index + 1),
            Category = 0,
            CountryCode = entry.Country,
            Rate = entry.Rate,
            ValidFrom = new DateOnly(2026, 1, 1),
            ValidTo = (DateOnly?)null
        }).ToArray();
    }
}
