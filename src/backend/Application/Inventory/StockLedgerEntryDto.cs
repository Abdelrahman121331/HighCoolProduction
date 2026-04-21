using ERP.Domain.Inventory;

namespace ERP.Application.Inventory;

public sealed record StockLedgerEntryDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    StockTransactionType TransactionType,
    SourceDocumentType SourceDocType,
    Guid SourceDocId,
    Guid? SourceLineId,
    string SourceDocumentNo,
    decimal QtyIn,
    decimal QtyOut,
    Guid UomId,
    string UomCode,
    string UomName,
    decimal BaseQty,
    decimal RunningBalanceQty,
    DateTime TransactionDate,
    decimal? UnitCost,
    decimal? TotalCost,
    DateTime CreatedAt,
    string CreatedBy);
