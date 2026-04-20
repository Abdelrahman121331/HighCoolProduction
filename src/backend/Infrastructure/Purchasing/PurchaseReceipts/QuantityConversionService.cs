using ERP.Application.Purchasing.PurchaseReceipts;
using ERP.Domain.MasterData;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Purchasing.PurchaseReceipts;

public sealed class QuantityConversionService(AppDbContext dbContext) : IQuantityConversionService
{
    private const int Scale = 6;
    private readonly Dictionary<(Guid FromUomId, Guid ToUomId), UomConversion?> _conversionCache = [];

    public async Task<decimal> ConvertAsync(decimal quantity, Guid fromUomId, Guid toUomId, CancellationToken cancellationToken)
    {
        if (fromUomId == Guid.Empty || toUomId == Guid.Empty)
        {
            throw new InvalidOperationException("Quantity conversion requires valid UOM references.");
        }

        if (fromUomId == toUomId)
        {
            return RoundToScale(quantity);
        }

        var key = (fromUomId, toUomId);
        if (!_conversionCache.TryGetValue(key, out var conversion))
        {
            conversion = await dbContext.UomConversions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    entity => entity.FromUomId == fromUomId &&
                              entity.ToUomId == toUomId &&
                              entity.IsActive,
                    cancellationToken);

            _conversionCache[key] = conversion;
        }

        if (conversion is null)
        {
            throw new InvalidOperationException("A required global UOM conversion could not be resolved.");
        }

        var converted = quantity * conversion.Factor;
        return conversion.RoundingMode switch
        {
            RoundingMode.None => RoundToScale(converted),
            RoundingMode.Round => decimal.Round(converted, Scale, MidpointRounding.AwayFromZero),
            RoundingMode.Floor => decimal.Floor(converted * 1_000_000m) / 1_000_000m,
            RoundingMode.Ceiling => decimal.Ceiling(converted * 1_000_000m) / 1_000_000m,
            _ => RoundToScale(converted)
        };
    }

    private static decimal RoundToScale(decimal value)
    {
        return decimal.Round(value, Scale, MidpointRounding.AwayFromZero);
    }
}
