namespace ERP.Application.Purchasing.ShortageReasonCodes;

public sealed record ShortageReasonCodeDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool AffectsSupplierBalance,
    bool AffectsStock,
    bool RequiresApproval);
