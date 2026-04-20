namespace ERP.Application.Purchasing.PurchaseReceipts;

public sealed record PurchaseReceiptLineComponentDto(
    Guid Id,
    Guid ComponentItemId,
    string ComponentItemCode,
    string ComponentItemName,
    decimal ExpectedQty,
    decimal ActualReceivedQty,
    Guid UomId,
    string UomCode,
    string UomName,
    Guid? ShortageReasonCodeId,
    string? ShortageReasonCodeCode,
    string? ShortageReasonCodeName,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
