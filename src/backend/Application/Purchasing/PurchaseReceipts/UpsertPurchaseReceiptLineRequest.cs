namespace ERP.Application.Purchasing.PurchaseReceipts;

public sealed record UpsertPurchaseReceiptLineRequest(
    int LineNo,
    Guid? PurchaseOrderLineId,
    Guid ItemId,
    decimal? OrderedQtySnapshot,
    decimal ReceivedQty,
    Guid UomId,
    string? Notes,
    IReadOnlyList<UpsertPurchaseReceiptLineComponentRequest> Components);
