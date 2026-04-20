using ERP.Application.Common.Exceptions;
using ERP.Application.MasterData.Items;
using ERP.Application.MasterData.UomConversions;
using ERP.Domain.MasterData;
using ERP.Infrastructure.MasterData.Items;
using ERP.Infrastructure.MasterData.UomConversions;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERP.Application.Tests;

public sealed class ItemMasterDataRulesTests
{
    [Fact]
    public void ItemValidator_ShouldRequireRowsWhenHasComponentsIsTrue()
    {
        var validator = new UpsertItemRequestValidator();
        var model = new UpsertItemRequest("ITM-1", "Assembly", Guid.NewGuid(), true, true, true, []);

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("at least one component row", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ItemService_ShouldRejectDuplicateComponentRows()
    {
        await using var dbContext = CreateDbContext();
        var (uom, parentItem, componentItem) = await SeedItemsAsync(dbContext);
        var service = new ItemService(dbContext);

        var request = new UpsertItemRequest(
            "ITM-NEW",
            "New Assembly",
            uom.Id,
            true,
            true,
            true,
            [
                new UpsertItemComponentRequest(componentItem.Id, uom.Id, 1m),
                new UpsertItemComponentRequest(componentItem.Id, uom.Id, 2m)
            ]);

        var exception = await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            service.CreateAsync(request, "tester", CancellationToken.None));

        Assert.Contains("duplicate component rows", exception.Message, StringComparison.OrdinalIgnoreCase);
        _ = parentItem;
    }

    [Fact]
    public async Task ItemService_ShouldRejectSelfReferencingComponentRows()
    {
        await using var dbContext = CreateDbContext();
        var (uom, parentItem, _) = await SeedItemsAsync(dbContext);
        var service = new ItemService(dbContext);

        var request = new UpsertItemRequest(
            "ITM-SELF",
            "Self Assembly",
            uom.Id,
            true,
            true,
            true,
            [new UpsertItemComponentRequest(parentItem.Id, uom.Id, 1m)]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(parentItem.Id, request, "tester", CancellationToken.None));

        Assert.Contains("same item", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ItemService_ShouldRequireGlobalConversionWhenComponentUomDiffersFromBase()
    {
        await using var dbContext = CreateDbContext();
        var (pieceUom, parentItem, componentItem) = await SeedItemsAsync(dbContext);

        var boxUom = new Uom
        {
            Code = "BOX",
            Name = "Box",
            Precision = 0,
            AllowsFraction = false,
            IsActive = true,
            CreatedBy = "seed"
        };

        dbContext.Uoms.Add(boxUom);
        await dbContext.SaveChangesAsync();

        var service = new ItemService(dbContext);
        var request = new UpsertItemRequest(
            "ITM-CONV",
            "Assembly With Box",
            pieceUom.Id,
            true,
            true,
            true,
            [new UpsertItemComponentRequest(componentItem.Id, boxUom.Id, 1m)]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(request, "tester", CancellationToken.None));

        Assert.Contains("global UOM conversion is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        _ = parentItem;
    }

    [Fact]
    public async Task UomConversionService_ShouldRejectDuplicateActivePair()
    {
        await using var dbContext = CreateDbContext();
        var (uom, _, _) = await SeedItemsAsync(dbContext);

        var alternateUom = new Uom
        {
            Code = "BOX",
            Name = "Box",
            Precision = 0,
            AllowsFraction = false,
            IsActive = true,
            CreatedBy = "seed"
        };

        dbContext.Uoms.Add(alternateUom);
        await dbContext.SaveChangesAsync();

        dbContext.UomConversions.Add(new UomConversion
        {
            FromUomId = alternateUom.Id,
            ToUomId = uom.Id,
            Factor = 12m,
            RoundingMode = RoundingMode.Round,
            IsActive = true,
            CreatedBy = "seed"
        });
        await dbContext.SaveChangesAsync();

        var service = new UomConversionService(dbContext);
        var request = new UpsertUomConversionRequest(
            alternateUom.Id,
            uom.Id,
            24m,
            RoundingMode.Round,
            true);

        var exception = await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            service.CreateAsync(request, "tester", CancellationToken.None));

        Assert.Contains("active conversion", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<(Uom uom, Item parentItem, Item componentItem)> SeedItemsAsync(AppDbContext dbContext)
    {
        var uom = new Uom
        {
            Code = "PCS",
            Name = "Pieces",
            Precision = 0,
            AllowsFraction = false,
            IsActive = true,
            CreatedBy = "seed"
        };

        dbContext.Uoms.Add(uom);
        await dbContext.SaveChangesAsync();

        var parentItem = new Item
        {
            Code = "ITM-PARENT",
            Name = "Parent Item",
            BaseUomId = uom.Id,
            IsActive = true,
            IsSellable = true,
            HasComponents = true,
            CreatedBy = "seed"
        };

        var componentItem = new Item
        {
            Code = "ITM-COMP",
            Name = "Component Item",
            BaseUomId = uom.Id,
            IsActive = true,
            IsSellable = false,
            HasComponents = false,
            CreatedBy = "seed"
        };

        dbContext.Items.AddRange(parentItem, componentItem);
        await dbContext.SaveChangesAsync();

        return (uom, parentItem, componentItem);
    }
}
