using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public sealed record OpenShortageQuery(
    string? Search,
    Guid? SupplierId,
    Guid? ItemId,
    Guid? ComponentItemId,
    bool? AffectsSupplierBalance,
    ShortageEntryStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate);
