using ERP.Domain.MasterData;

namespace ERP.Application.MasterData.UomConversions;

public sealed record UpsertUomConversionRequest(
    Guid FromUomId,
    Guid ToUomId,
    decimal Factor,
    RoundingMode RoundingMode,
    bool IsActive);
