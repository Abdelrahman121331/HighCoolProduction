using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShortageResolutionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "open_amount",
                table: "shortage_ledger_entries",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "open_qty",
                table: "shortage_ledger_entries",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "resolved_amount",
                table: "shortage_ledger_entries",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "resolved_qty",
                table: "shortage_ledger_entries",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "shortage_value",
                table: "shortage_ledger_entries",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "shortage_resolutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    resolution_no = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    resolution_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    resolution_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    currency = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    approved_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shortage_resolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shortage_resolutions_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplier_statement_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    effect_type = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    source_doc_type = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    source_doc_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    source_line_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    amount_delta = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    running_balance = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    currency = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_statement_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_statement_entries_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shortage_resolution_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    resolution_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    shortage_ledger_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    allocated_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    allocated_amount = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    valuation_rate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    allocation_method = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    sequence_no = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shortage_resolution_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shortage_resolution_allocations_shortage_ledger_entries_shortage_ledger_id",
                        column: x => x.shortage_ledger_id,
                        principalTable: "shortage_ledger_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shortage_resolution_allocations_shortage_resolutions_resolution_id",
                        column: x => x.resolution_id,
                        principalTable: "shortage_resolutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_status",
                table: "shortage_ledger_entries",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolution_allocations_resolution_id",
                table: "shortage_resolution_allocations",
                column: "resolution_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolution_allocations_resolution_id_sequence_no",
                table: "shortage_resolution_allocations",
                columns: new[] { "resolution_id", "sequence_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolution_allocations_resolution_id_shortage_ledger_id",
                table: "shortage_resolution_allocations",
                columns: new[] { "resolution_id", "shortage_ledger_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolution_allocations_shortage_ledger_id",
                table: "shortage_resolution_allocations",
                column: "shortage_ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolutions_resolution_no",
                table: "shortage_resolutions",
                column: "resolution_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shortage_resolutions_supplier_id_resolution_type_status_resolution_date",
                table: "shortage_resolutions",
                columns: new[] { "supplier_id", "resolution_type", "status", "resolution_date" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_statement_entries_source_doc_id_source_line_id_effect_type",
                table: "supplier_statement_entries",
                columns: new[] { "source_doc_id", "source_line_id", "effect_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_statement_entries_supplier_id_transaction_date",
                table: "supplier_statement_entries",
                columns: new[] { "supplier_id", "transaction_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shortage_resolution_allocations");

            migrationBuilder.DropTable(
                name: "supplier_statement_entries");

            migrationBuilder.DropTable(
                name: "shortage_resolutions");

            migrationBuilder.DropIndex(
                name: "IX_shortage_ledger_entries_status",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "open_amount",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "open_qty",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "resolved_amount",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "resolved_qty",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "shortage_value",
                table: "shortage_ledger_entries");
        }
    }
}
