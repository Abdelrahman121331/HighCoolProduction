namespace ERP.Application.Purchasing.PurchaseOrders;

public sealed record UpsertPurchaseOrderRequest(
    string? PoNo,
    Guid SupplierId,
    DateTime? OrderDate,
    DateTime? ExpectedDate,
    string? Notes,
    IReadOnlyList<UpsertPurchaseOrderLineRequest> Lines);
