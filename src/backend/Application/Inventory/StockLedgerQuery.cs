using ERP.Domain.Inventory;

namespace ERP.Application.Inventory;

public sealed record StockLedgerQuery(
    string? Search,
    Guid? ItemId,
    Guid? WarehouseId,
    StockTransactionType? TransactionType,
    DateTime? FromDate,
    DateTime? ToDate);
