using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrdersAndPurchaseOrderLinkedReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_ledger_entries_source_doc_id_source_doc_line_id_transaction_type",
                table: "stock_ledger_entries");

            migrationBuilder.DropColumn(
                name: "source_doc_line_id",
                table: "stock_ledger_entries");

            migrationBuilder.RenameColumn(
                name: "ordered_qty",
                table: "purchase_receipt_lines",
                newName: "ordered_qty_snapshot");

            migrationBuilder.AddColumn<Guid>(
                name: "source_line_id",
                table: "stock_ledger_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_cost",
                table: "stock_ledger_entries",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "unit_cost",
                table: "stock_ledger_entries",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_approval",
                table: "shortage_reason_codes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "approval_status",
                table: "shortage_ledger_entries",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "purchase_order_id",
                table: "shortage_ledger_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "purchase_order_line_id",
                table: "shortage_ledger_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "purchase_order_id",
                table: "purchase_receipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "purchase_order_line_id",
                table: "purchase_receipt_lines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    po_no = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expected_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    line_no = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ordered_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_uoms_uom_id",
                        column: x => x.uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_source_doc_id_source_line_id_transaction_type",
                table: "stock_ledger_entries",
                columns: new[] { "source_doc_id", "source_line_id", "transaction_type" },
                unique: true,
                filter: "[source_line_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_purchase_order_id",
                table: "shortage_ledger_entries",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_shortage_ledger_entries_purchase_order_line_id",
                table: "shortage_ledger_entries",
                column: "purchase_order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_purchase_order_id",
                table: "purchase_receipts",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_lines_purchase_order_line_id",
                table: "purchase_receipt_lines",
                column: "purchase_order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_item_id",
                table: "purchase_order_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_id_line_no",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_uom_id",
                table: "purchase_order_lines",
                column: "uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_order_date",
                table: "purchase_orders",
                column: "order_date");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_po_no",
                table: "purchase_orders",
                column: "po_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_status",
                table: "purchase_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_supplier_id",
                table: "purchase_orders",
                column: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_receipt_lines_purchase_order_lines_purchase_order_line_id",
                table: "purchase_receipt_lines",
                column: "purchase_order_line_id",
                principalTable: "purchase_order_lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_receipts_purchase_orders_purchase_order_id",
                table: "purchase_receipts",
                column: "purchase_order_id",
                principalTable: "purchase_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_shortage_ledger_entries_purchase_order_lines_purchase_order_line_id",
                table: "shortage_ledger_entries",
                column: "purchase_order_line_id",
                principalTable: "purchase_order_lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_shortage_ledger_entries_purchase_orders_purchase_order_id",
                table: "shortage_ledger_entries",
                column: "purchase_order_id",
                principalTable: "purchase_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_receipt_lines_purchase_order_lines_purchase_order_line_id",
                table: "purchase_receipt_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_receipts_purchase_orders_purchase_order_id",
                table: "purchase_receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_shortage_ledger_entries_purchase_order_lines_purchase_order_line_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_shortage_ledger_entries_purchase_orders_purchase_order_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropTable(
                name: "purchase_order_lines");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_stock_ledger_entries_source_doc_id_source_line_id_transaction_type",
                table: "stock_ledger_entries");

            migrationBuilder.DropIndex(
                name: "IX_shortage_ledger_entries_purchase_order_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropIndex(
                name: "IX_shortage_ledger_entries_purchase_order_line_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropIndex(
                name: "IX_purchase_receipts_purchase_order_id",
                table: "purchase_receipts");

            migrationBuilder.DropIndex(
                name: "IX_purchase_receipt_lines_purchase_order_line_id",
                table: "purchase_receipt_lines");

            migrationBuilder.DropColumn(
                name: "source_line_id",
                table: "stock_ledger_entries");

            migrationBuilder.DropColumn(
                name: "total_cost",
                table: "stock_ledger_entries");

            migrationBuilder.DropColumn(
                name: "unit_cost",
                table: "stock_ledger_entries");

            migrationBuilder.DropColumn(
                name: "requires_approval",
                table: "shortage_reason_codes");

            migrationBuilder.DropColumn(
                name: "approval_status",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "purchase_order_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "purchase_order_line_id",
                table: "shortage_ledger_entries");

            migrationBuilder.DropColumn(
                name: "purchase_order_id",
                table: "purchase_receipts");

            migrationBuilder.DropColumn(
                name: "purchase_order_line_id",
                table: "purchase_receipt_lines");

            migrationBuilder.RenameColumn(
                name: "ordered_qty_snapshot",
                table: "purchase_receipt_lines",
                newName: "ordered_qty");

            migrationBuilder.AddColumn<Guid>(
                name: "source_doc_line_id",
                table: "stock_ledger_entries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_entries_source_doc_id_source_doc_line_id_transaction_type",
                table: "stock_ledger_entries",
                columns: new[] { "source_doc_id", "source_doc_line_id", "transaction_type" },
                unique: true);
        }
    }
}
