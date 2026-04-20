using ERP.Application.Common.Exceptions;
using ERP.Application.MasterData.UomConversions;
using ERP.Domain.MasterData;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.MasterData.UomConversions;

public sealed class UomConversionService(AppDbContext dbContext) : IUomConversionService
{
    public async Task<IReadOnlyList<UomConversionDto>> ListAsync(UomConversionListQuery query, CancellationToken cancellationToken)
    {
        var conversions = IncludeReferences();

        if (query.IsActive.HasValue)
        {
            conversions = conversions.Where(entity => entity.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            conversions = conversions.Where(entity =>
                entity.FromUom!.Code.Contains(search) ||
                entity.FromUom.Name.Contains(search) ||
                entity.ToUom!.Code.Contains(search) ||
                entity.ToUom.Name.Contains(search));
        }

        return await conversions
            .OrderBy(entity => entity.FromUom!.Code)
            .ThenBy(entity => entity.ToUom!.Code)
            .Select(entity => ToDto(entity))
            .ToListAsync(cancellationToken);
    }

    public Task<UomConversionDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return IncludeReferences()
            .Where(entity => entity.Id == id)
            .Select(entity => ToDto(entity))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UomConversionDto> CreateAsync(
        UpsertUomConversionRequest request,
        string actor,
        CancellationToken cancellationToken)
    {
        await ValidateRuleAsync(request, null, cancellationToken);

        var conversion = new UomConversion
        {
            FromUomId = request.FromUomId,
            ToUomId = request.ToUomId,
            Factor = request.Factor,
            RoundingMode = request.RoundingMode,
            IsActive = request.IsActive,
            CreatedBy = actor
        };

        dbContext.UomConversions.Add(conversion);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredAsync(conversion.Id, cancellationToken);
    }

    public async Task<UomConversionDto?> UpdateAsync(
        Guid id,
        UpsertUomConversionRequest request,
        string actor,
        CancellationToken cancellationToken)
    {
        var conversion = await dbContext.UomConversions.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (conversion is null)
        {
            return null;
        }

        await ValidateRuleAsync(request, id, cancellationToken);

        conversion.FromUomId = request.FromUomId;
        conversion.ToUomId = request.ToUomId;
        conversion.Factor = request.Factor;
        conversion.RoundingMode = request.RoundingMode;
        conversion.IsActive = request.IsActive;
        conversion.UpdatedBy = actor;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredAsync(conversion.Id, cancellationToken);
    }

    public async Task<bool> DeactivateAsync(Guid id, string actor, CancellationToken cancellationToken)
    {
        var conversion = await dbContext.UomConversions.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (conversion is null)
        {
            return false;
        }

        if (!conversion.IsActive)
        {
            return true;
        }

        conversion.IsActive = false;
        conversion.UpdatedBy = actor;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<UomConversion> IncludeReferences()
    {
        return dbContext.UomConversions
            .AsNoTracking()
            .Include(entity => entity.FromUom)
            .Include(entity => entity.ToUom);
    }

    private async Task ValidateRuleAsync(UpsertUomConversionRequest request, Guid? currentId, CancellationToken cancellationToken)
    {
        if (request.FromUomId == request.ToUomId)
        {
            throw new InvalidOperationException("From UOM and To UOM must be different.");
        }

        var uomsExist = await dbContext.Uoms.CountAsync(
            entity => entity.Id == request.FromUomId || entity.Id == request.ToUomId,
            cancellationToken);

        if (uomsExist != 2)
        {
            throw new InvalidOperationException("One or more UOM references were not found.");
        }

        if (request.IsActive)
        {
            var activePairExists = await dbContext.UomConversions.AnyAsync(
                entity => entity.FromUomId == request.FromUomId &&
                          entity.ToUomId == request.ToUomId &&
                          entity.IsActive &&
                          entity.Id != currentId,
                cancellationToken);

            if (activePairExists)
            {
                throw new DuplicateEntityException("An active conversion already exists for this UOM pair.");
            }
        }
    }

    private async Task<UomConversionDto> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        var conversion = await GetAsync(id, cancellationToken);
        return conversion ?? throw new InvalidOperationException("UOM conversion was not found after save.");
    }

    private static UomConversionDto ToDto(UomConversion entity)
    {
        return new UomConversionDto(
            entity.Id,
            entity.FromUomId,
            entity.FromUom?.Code ?? string.Empty,
            entity.FromUom?.Name ?? string.Empty,
            entity.ToUomId,
            entity.ToUom?.Code ?? string.Empty,
            entity.ToUom?.Name ?? string.Empty,
            entity.Factor,
            entity.RoundingMode,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
