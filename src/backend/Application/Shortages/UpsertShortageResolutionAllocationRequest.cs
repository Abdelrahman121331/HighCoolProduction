namespace ERP.Application.Shortages;

public sealed record UpsertShortageResolutionAllocationRequest(
    Guid ShortageLedgerId,
    decimal? AllocatedQty,
    decimal? AllocatedAmount,
    decimal? ValuationRate,
    string? AllocationMethod,
    int SequenceNo);
