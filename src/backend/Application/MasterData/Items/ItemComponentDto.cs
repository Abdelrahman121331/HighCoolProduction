namespace ERP.Application.MasterData.Items;

public sealed record ItemComponentDto(
    Guid Id,
    Guid ItemId,
    Guid ComponentItemId,
    string ComponentItemCode,
    string ComponentItemName,
    Guid ComponentBaseUomId,
    string ComponentBaseUomCode,
    Guid UomId,
    string UomCode,
    string UomName,
    decimal Quantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
