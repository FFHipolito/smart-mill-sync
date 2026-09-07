using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMillSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gas_telemetry_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlowRateNm3PerHour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TemperatureCelsius = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ThermalDeviationPercentage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gas_telemetry_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wood_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckPlate = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ForestOrigin = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    WoodSpecies = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    GrossWeight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TareWeight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MoisturePercentage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ArrivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wood_deliveries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gas_telemetry_records_RecordedAtUtc",
                table: "gas_telemetry_records",
                column: "RecordedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_wood_deliveries_ArrivedAtUtc",
                table: "wood_deliveries",
                column: "ArrivedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_wood_deliveries_Status",
                table: "wood_deliveries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gas_telemetry_records");

            migrationBuilder.DropTable(
                name: "wood_deliveries");
        }
    }
}
