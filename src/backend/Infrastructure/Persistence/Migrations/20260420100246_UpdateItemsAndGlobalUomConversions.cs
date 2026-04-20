using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemsAndGlobalUomConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_components_items_parent_item_id",
                table: "item_components");

            migrationBuilder.DropTable(
                name: "item_uom_conversions");

            migrationBuilder.DropIndex(
                name: "IX_item_components_parent_item_id_component_item_id",
                table: "item_components");

            migrationBuilder.RenameColumn(
                name: "is_component",
                table: "items",
                newName: "has_components");

            migrationBuilder.RenameColumn(
                name: "parent_item_id",
                table: "item_components",
                newName: "item_id");

            migrationBuilder.AddColumn<Guid>(
                name: "uom_id",
                table: "item_components",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE item_components
                SET uom_id = (
                    SELECT base_uom_id
                    FROM items
                    WHERE items.Id = item_components.component_item_id
                )
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "uom_id",
                table: "item_components",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "uom_conversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    from_uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    to_uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    factor = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    rounding_mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uom_conversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uom_conversions_uoms_from_uom_id",
                        column: x => x.from_uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_uom_conversions_uoms_to_uom_id",
                        column: x => x.to_uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_components_item_id_component_item_id",
                table: "item_components",
                columns: new[] { "item_id", "component_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_components_uom_id",
                table: "item_components",
                column: "uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_uom_conversions_from_uom_id_to_uom_id_is_active",
                table: "uom_conversions",
                columns: new[] { "from_uom_id", "to_uom_id", "is_active" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_uom_conversions_to_uom_id",
                table: "uom_conversions",
                column: "to_uom_id");

            migrationBuilder.AddForeignKey(
                name: "FK_item_components_items_item_id",
                table: "item_components",
                column: "item_id",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_item_components_uoms_uom_id",
                table: "item_components",
                column: "uom_id",
                principalTable: "uoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_components_items_item_id",
                table: "item_components");

            migrationBuilder.DropForeignKey(
                name: "FK_item_components_uoms_uom_id",
                table: "item_components");

            migrationBuilder.DropTable(
                name: "uom_conversions");

            migrationBuilder.DropIndex(
                name: "IX_item_components_item_id_component_item_id",
                table: "item_components");

            migrationBuilder.DropIndex(
                name: "IX_item_components_uom_id",
                table: "item_components");

            migrationBuilder.DropColumn(
                name: "uom_id",
                table: "item_components");

            migrationBuilder.RenameColumn(
                name: "has_components",
                table: "items",
                newName: "is_component");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "item_components",
                newName: "parent_item_id");

            migrationBuilder.CreateTable(
                name: "item_uom_conversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    from_uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    to_uom_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    factor = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    min_fraction = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    rounding_mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_uom_conversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_uom_conversions_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_item_uom_conversions_uoms_from_uom_id",
                        column: x => x.from_uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_item_uom_conversions_uoms_to_uom_id",
                        column: x => x.to_uom_id,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_components_parent_item_id_component_item_id",
                table: "item_components",
                columns: new[] { "parent_item_id", "component_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_uom_conversions_from_uom_id",
                table: "item_uom_conversions",
                column: "from_uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_uom_conversions_item_id_from_uom_id_to_uom_id_is_active",
                table: "item_uom_conversions",
                columns: new[] { "item_id", "from_uom_id", "to_uom_id", "is_active" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_uom_conversions_to_uom_id",
                table: "item_uom_conversions",
                column: "to_uom_id");

            migrationBuilder.AddForeignKey(
                name: "FK_item_components_items_parent_item_id",
                table: "item_components",
                column: "parent_item_id",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
