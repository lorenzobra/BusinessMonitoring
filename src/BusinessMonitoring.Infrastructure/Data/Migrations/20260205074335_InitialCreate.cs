using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessMonitoring.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessingErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingErrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceType = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    ActivationDate = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    ImportedFromFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedFromFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ProcessingStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    ProcessingCompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Pending"),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ValidRows = table.Column<int>(type: "int", nullable: false),
                    InvalidRows = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingError_BatchId",
                table: "ProcessingErrors",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingError_CreatedAt",
                table: "ProcessingErrors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingError_FileName",
                table: "ProcessingErrors",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ActivationDate",
                table: "Services",
                column: "ActivationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CustomerId_ServiceType_ActivationDate",
                table: "Services",
                columns: new[] { "CustomerId", "ServiceType", "ActivationDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_CustomerId_Status",
                table: "Services",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ExpirationDate",
                table: "Services",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceType_Status",
                table: "Services",
                columns: new[] { "ServiceType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UploadHistory_BatchId",
                table: "UploadHistories",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadHistory_FileName",
                table: "UploadHistories",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_UploadHistory_UploadedAt",
                table: "UploadHistories",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingErrors");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "UploadHistories");
        }
    }
}
