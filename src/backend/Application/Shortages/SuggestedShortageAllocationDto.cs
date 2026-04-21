namespace ERP.Application.Shortages;

public sealed record SuggestedShortageAllocationDto(
    Guid ShortageLedgerId,
    int SequenceNo,
    string AllocationMethod,
    decimal? AllocatedQty,
    decimal? AllocatedAmount,
    decimal? ValuationRate,
    decimal OpenQty,
    decimal? OpenAmount,
    string PurchaseReceiptNo,
    DateTime ReceiptDate,
    string ItemCode,
    string ComponentItemCode);
