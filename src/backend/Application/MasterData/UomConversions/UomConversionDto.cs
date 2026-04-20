using ERP.Domain.MasterData;

namespace ERP.Application.MasterData.UomConversions;

public sealed record UomConversionDto(
    Guid Id,
    Guid FromUomId,
    string FromUomCode,
    string FromUomName,
    Guid ToUomId,
    string ToUomCode,
    string ToUomName,
    decimal Factor,
    RoundingMode RoundingMode,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
