namespace ERP.Application.Inventory;

public sealed record StockBalanceDto(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid BaseUomId,
    string BaseUomCode,
    string BaseUomName,
    decimal BalanceQty,
    DateTime? LastTransactionDate);
