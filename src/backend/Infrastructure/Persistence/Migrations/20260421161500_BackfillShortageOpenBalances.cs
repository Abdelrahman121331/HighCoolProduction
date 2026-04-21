using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    public partial class BackfillShortageOpenBalances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE shortage_ledger_entries
                SET resolved_qty = COALESCE(resolved_qty, 0),
                    resolved_amount = COALESCE(resolved_amount, 0),
                    open_qty = CASE
                        WHEN status = 'Canceled' THEN 0
                        WHEN status = 'Resolved' THEN 0
                        ELSE max(shortage_qty - COALESCE(resolved_qty, 0), 0)
                    END,
                    open_amount = CASE
                        WHEN shortage_value IS NULL THEN NULL
                        WHEN status = 'Canceled' THEN 0
                        WHEN status = 'Resolved' THEN 0
                        ELSE max(shortage_value - COALESCE(resolved_amount, 0), 0)
                    END,
                    status = CASE
                        WHEN status = 'Canceled' THEN 'Canceled'
                        WHEN max(shortage_qty - COALESCE(resolved_qty, 0), 0) = 0 AND shortage_qty > 0 THEN 'Resolved'
                        WHEN COALESCE(resolved_qty, 0) > 0 OR COALESCE(resolved_amount, 0) > 0 THEN 'PartiallyResolved'
                        ELSE 'Open'
                    END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
