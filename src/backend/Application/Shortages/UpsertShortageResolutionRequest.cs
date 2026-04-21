using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public sealed record UpsertShortageResolutionRequest(
    string? ResolutionNo,
    Guid SupplierId,
    ShortageResolutionType? ResolutionType,
    DateTime? ResolutionDate,
    decimal? TotalQty,
    decimal? TotalAmount,
    string? Currency,
    string? Notes,
    IReadOnlyList<UpsertShortageResolutionAllocationRequest> Allocations);
