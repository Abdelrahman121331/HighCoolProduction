namespace ERP.Application.MasterData.Items;

public sealed record ItemDto(
    Guid Id,
    string Code,
    string Name,
    Guid BaseUomId,
    string BaseUomCode,
    string BaseUomName,
    bool IsActive,
    bool IsSellable,
    bool HasComponents,
    IReadOnlyList<ItemComponentDto> Components,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
