namespace ERP.Application.Purchasing.PurchaseReceipts;

public sealed record UpsertPurchaseReceiptLineComponentRequest(
    Guid ComponentItemId,
    decimal ActualReceivedQty,
    Guid UomId,
    Guid? ShortageReasonCodeId,
    string? Notes);
