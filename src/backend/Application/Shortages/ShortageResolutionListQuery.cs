using ERP.Domain.Common;
using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public sealed record ShortageResolutionListQuery(
    string? Search,
    Guid? SupplierId,
    ShortageResolutionType? ResolutionType,
    DocumentStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate);
