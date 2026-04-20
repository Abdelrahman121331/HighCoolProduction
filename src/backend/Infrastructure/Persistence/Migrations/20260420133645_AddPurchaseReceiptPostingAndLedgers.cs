using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseReceiptPostingAndLedgers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "shortage_reason_code_id",
                table: "purchase_receipt_line_components",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "shortage_reason_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    affects_supplier_balance = table.Column<bool>(type: "bit", nullable: false),
                    affects_stock = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shortage_reason_codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stock_ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    source_doc_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    source_doc_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_doc_line_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    qty_in = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    qty_out = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    base_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    running_balance_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_ledger_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_ledger_entries_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_ledger_entries_uoms_uom_id",
                        column: x => x.uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_ledger_entries_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shortage_ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_receipt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_receipt_line_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    component_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    expected_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    actual_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    shortage_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    shortage_reason_code_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    affects_supplier_balance = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shortage_ledger_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shortage_ledger_entries_items_component_item_id",
                        column: x => x.component_item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shortage_ledger_entries_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shortage_ledger_entries_purchase_receipt_lines_purchase_receipt_line_id",
                        column: x => x.purchase_receipt_line_id,
                        principalTable: "purchase_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shortage_ledger_entries_purchase_receipts_purchase_receipt_id",
                        column: x => x.purchase_receipt_id,
                        principalTable: "purchase_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shortage_ledger_entries_shortage_reason_codes_shortage_reason_code_id",
                        column: x => x.shortage_reason_code_id,
                        principalTable: "shortage_reason_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_line_components_shortage_reason_code_id",
                table: "purchase_receipt_line_components",
                column: "shortage_reason_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_component_item_id",
                table: "shortage_ledger_entries",
                column: "component_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_item_id",
                table: "shortage_ledger_entries",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_purchase_receipt_id",
                table: "shortage_ledger_entries",
                column: "purchase_receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_purchase_receipt_line_id",
                table: "shortage_ledger_entries",
                column: "purchase_receipt_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_purchase_receipt_line_id_component_item_id",
                table: "shortage_ledger_entries",
                columns: new[] { "purchase_receipt_line_id", "component_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_shortage_reason_code_id",
                table: "shortage_ledger_entries",
                column: "shortage_reason_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_reason_codes_code",
                table: "shortage_reason_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_item_id_warehouse_id_transaction_date",
                table: "stock_ledger_entries",
                columns: new[] { "item_id", "warehouse_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_source_doc_id_source_doc_line_id_transaction_type",
                table: "stock_ledger_entries",
                columns: new[] { "source_doc_id", "source_doc_line_id", "transaction_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_uom_id",
                table: "stock_ledger_entries",
                column: "uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_warehouse_id",
                table: "stock_ledger_entries",
                column: "warehouse_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_receipt_line_components_shortage_reason_codes_shortage_reason_code_id",
                table: "purchase_receipt_line_components",
                column: "shortage_reason_code_id",
                principalTable: "shortage_reason_codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_receipt_line_components_shortage_reason_codes_shortage_reason_code_id",
                table: "purchase_receipt_line_components");

            migrationBuilder.DropTable(
                name: "shortage_ledger_entries");

            migrationBuilder.DropTable(
                name: "stock_ledger_entries");

            migrationBuilder.DropTable(
                name: "shortage_reason_codes");

            migrationBuilder.DropIndex(
                name: "IX_purchase_receipt_line_components_shortage_reason_code_id",
                table: "purchase_receipt_line_components");

            migrationBuilder.DropColumn(
                name: "shortage_reason_code_id",
                table: "purchase_receipt_line_components");
        }
    }
}
