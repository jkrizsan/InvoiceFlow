using InvoiceFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InvoiceDbContext))]
[Migration("202609020001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Invoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OwnerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                SellerName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                SellerBusinessId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                SellerVatNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                SellerAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                SellerCountryCode = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                CustomerName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                CustomerEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                CustomerAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                CustomerCountryCode = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                CustomerType = table.Column<int>(type: "integer", nullable: false),
                CustomerVatNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                VatTreatment = table.Column<int>(type: "integer", nullable: false),
                NetTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                VatTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                GrossTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Version = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Invoices", x => x.Id));

        migrationBuilder.CreateTable(
            name: "VatRates",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CountryCode = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                Category = table.Column<int>(type: "integer", nullable: false),
                Rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_VatRates", x => x.Id));

        migrationBuilder.CreateTable(
            name: "InvoiceLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                VatCategory = table.Column<int>(type: "integer", nullable: false),
                VatRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                VatAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_InvoiceLines_Invoices_InvoiceId",
                    column: x => x.InvoiceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "VatRates",
            columns: ["Id", "Category", "CountryCode", "Rate", "ValidFrom", "ValidTo"],
            columnTypes:
            [
                "integer",
                "integer",
                "character(2)",
                "numeric(5,2)",
                "date",
                "date"
            ],
            values: new object[,]
            {
                { -1, 0, "AT", 20m, new DateOnly(2026, 1, 1), null },
                { -2, 0, "BE", 21m, new DateOnly(2026, 1, 1), null },
                { -3, 0, "BG", 20m, new DateOnly(2026, 1, 1), null },
                { -4, 0, "HR", 25m, new DateOnly(2026, 1, 1), null },
                { -5, 0, "CY", 19m, new DateOnly(2026, 1, 1), null },
                { -6, 0, "CZ", 21m, new DateOnly(2026, 1, 1), null },
                { -7, 0, "DK", 25m, new DateOnly(2026, 1, 1), null },
                { -8, 0, "EE", 24m, new DateOnly(2026, 1, 1), null },
                { -9, 0, "FI", 25.5m, new DateOnly(2026, 1, 1), null },
                { -10, 0, "FR", 20m, new DateOnly(2026, 1, 1), null },
                { -11, 0, "DE", 19m, new DateOnly(2026, 1, 1), null },
                { -12, 0, "GR", 24m, new DateOnly(2026, 1, 1), null },
                { -13, 0, "HU", 27m, new DateOnly(2026, 1, 1), null },
                { -14, 0, "IE", 23m, new DateOnly(2026, 1, 1), null },
                { -15, 0, "IT", 22m, new DateOnly(2026, 1, 1), null },
                { -16, 0, "LV", 21m, new DateOnly(2026, 1, 1), null },
                { -17, 0, "LT", 21m, new DateOnly(2026, 1, 1), null },
                { -18, 0, "LU", 17m, new DateOnly(2026, 1, 1), null },
                { -19, 0, "MT", 18m, new DateOnly(2026, 1, 1), null },
                { -20, 0, "NL", 21m, new DateOnly(2026, 1, 1), null },
                { -21, 0, "PL", 23m, new DateOnly(2026, 1, 1), null },
                { -22, 0, "PT", 23m, new DateOnly(2026, 1, 1), null },
                { -23, 0, "RO", 21m, new DateOnly(2026, 1, 1), null },
                { -24, 0, "SK", 23m, new DateOnly(2026, 1, 1), null },
                { -25, 0, "SI", 22m, new DateOnly(2026, 1, 1), null },
                { -26, 0, "ES", 21m, new DateOnly(2026, 1, 1), null },
                { -27, 0, "SE", 25m, new DateOnly(2026, 1, 1), null }
            });

        migrationBuilder.CreateIndex(
            name: "IX_InvoiceLines_InvoiceId",
            table: "InvoiceLines",
            column: "InvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_Number",
            table: "Invoices",
            column: "Number",
            unique: true,
            filter: "\"Number\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_OwnerId_Status",
            table: "Invoices",
            columns: ["OwnerId", "Status"]);

        migrationBuilder.CreateIndex(
            name: "IX_VatRates_CountryCode_Category_ValidFrom",
            table: "VatRates",
            columns: ["CountryCode", "Category", "ValidFrom"],
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InvoiceLines");
        migrationBuilder.DropTable(name: "VatRates");
        migrationBuilder.DropTable(name: "Invoices");
    }
}
