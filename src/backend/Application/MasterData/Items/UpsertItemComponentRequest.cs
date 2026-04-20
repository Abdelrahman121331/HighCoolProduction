namespace ERP.Application.MasterData.Items;

public sealed record UpsertItemComponentRequest(
    Guid ComponentItemId,
    Guid UomId,
    decimal Quantity);
