using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseReceiptDraftModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receipt_no = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receipt_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_receipts_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_receipts_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_receipt_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_receipt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    line_no = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ordered_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    received_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_receipt_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_lines_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_lines_purchase_receipts_purchase_receipt_id",
                        column: x => x.purchase_receipt_id,
                        principalTable: "purchase_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_lines_uoms_uom_id",
                        column: x => x.uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_receipt_line_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_receipt_line_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    component_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actual_received_qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_receipt_line_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_line_components_items_component_item_id",
                        column: x => x.component_item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_line_components_purchase_receipt_lines_purchase_receipt_line_id",
                        column: x => x.purchase_receipt_line_id,
                        principalTable: "purchase_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_line_components_uoms_uom_id",
                        column: x => x.uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_line_components_component_item_id",
                table: "purchase_receipt_line_components",
                column: "component_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_line_components_purchase_receipt_line_id_component_item_id",
                table: "purchase_receipt_line_components",
                columns: new[] { "purchase_receipt_line_id", "component_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_line_components_uom_id",
                table: "purchase_receipt_line_components",
                column: "uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_lines_item_id",
                table: "purchase_receipt_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_lines_purchase_receipt_id_line_no",
                table: "purchase_receipt_lines",
                columns: new[] { "purchase_receipt_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_lines_uom_id",
                table: "purchase_receipt_lines",
                column: "uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_receipt_date",
                table: "purchase_receipts",
                column: "receipt_date");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_receipt_no",
                table: "purchase_receipts",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_status",
                table: "purchase_receipts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_supplier_id",
                table: "purchase_receipts",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_warehouse_id",
                table: "purchase_receipts",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_receipt_line_components");

            migrationBuilder.DropTable(
                name: "purchase_receipt_lines");

            migrationBuilder.DropTable(
                name: "purchase_receipts");
        }
    }
}
