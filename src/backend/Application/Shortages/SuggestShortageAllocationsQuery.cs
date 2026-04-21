using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public sealed record SuggestShortageAllocationsQuery(
    Guid SupplierId,
    ShortageResolutionType ResolutionType,
    decimal? TotalQty,
    decimal? TotalAmount);
