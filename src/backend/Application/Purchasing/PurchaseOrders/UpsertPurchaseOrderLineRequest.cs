namespace ERP.Application.Purchasing.PurchaseOrders;

public sealed record UpsertPurchaseOrderLineRequest(
    int LineNo,
    Guid ItemId,
    decimal OrderedQty,
    Guid UomId,
    string? Notes);
