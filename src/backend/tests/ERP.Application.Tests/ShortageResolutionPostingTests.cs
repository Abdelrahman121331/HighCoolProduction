using ERP.Application.Shortages;
using ERP.Domain.Common;
using ERP.Domain.Inventory;
using ERP.Domain.MasterData;
using ERP.Domain.Purchasing;
using ERP.Domain.Shortages;
using ERP.Domain.Statements;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Shortages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERP.Application.Tests;

public sealed class ShortageResolutionPostingTests
{
    [Fact]
    public async Task PostAsync_ShouldFullyResolvePhysicalShortageAndCreateStockLedgerEntry()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 5m);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedQty: 5m)
            });

        var service = CreatePostingService(dbContext);

        var result = await service.PostAsync(resolution.Id, "tester", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(DocumentStatus.Posted, result!.Status);

        var updatedShortage = await dbContext.ShortageLedgerEntries.SingleAsync();
        Assert.Equal(5m, updatedShortage.ResolvedQty);
        Assert.Equal(0m, updatedShortage.OpenQty);
        Assert.Equal(ShortageEntryStatus.Resolved, updatedShortage.Status);

        var stockEntry = await dbContext.StockLedgerEntries.SingleAsync();
        Assert.Equal(StockTransactionType.ShortagePhysicalResolution, stockEntry.TransactionType);
        Assert.Equal(SourceDocumentType.ShortageResolution, stockEntry.SourceDocType);
        Assert.Equal(5m, stockEntry.QtyIn);
        Assert.Equal(references.ComponentItem.BaseUomId, stockEntry.UomId);
    }

    [Fact]
    public async Task PostAsync_ShouldPartiallyResolvePhysicalShortage()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 10m);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedQty: 4m)
            });

        var service = CreatePostingService(dbContext);

        await service.PostAsync(resolution.Id, "tester", CancellationToken.None);

        var updatedShortage = await dbContext.ShortageLedgerEntries.SingleAsync();
        Assert.Equal(4m, updatedShortage.ResolvedQty);
        Assert.Equal(6m, updatedShortage.OpenQty);
        Assert.Equal(ShortageEntryStatus.PartiallyResolved, updatedShortage.Status);
    }

    [Fact]
    public async Task PostAsync_ShouldResolveMultipleShortageRowsInOnePhysicalResolution()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var first = await CreateShortageAsync(dbContext, references, shortageQty: 3m);
        var second = await CreateShortageAsync(dbContext, references, shortageQty: 2m, suffix: "0002");
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(first.Id, AllocatedQty: 3m),
                new AllocationSeed(second.Id, AllocatedQty: 2m)
            });

        var service = CreatePostingService(dbContext);

        await service.PostAsync(resolution.Id, "tester", CancellationToken.None);

        Assert.Equal(2, await dbContext.StockLedgerEntries.CountAsync());
        Assert.All(await dbContext.ShortageLedgerEntries.OrderBy(entity => entity.CreatedAt).ToListAsync(), shortage =>
        {
            Assert.Equal(0m, shortage.OpenQty);
            Assert.Equal(ShortageEntryStatus.Resolved, shortage.Status);
        });
    }

    [Fact]
    public async Task PostAsync_ShouldCreateSupplierStatementForFinancialResolution()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 10m, affectsSupplierBalance: true);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Financial,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedAmount: 40m, ValuationRate: 10m)
            });

        var service = CreatePostingService(dbContext);

        await service.PostAsync(resolution.Id, "tester", CancellationToken.None);

        var updatedShortage = await dbContext.ShortageLedgerEntries.SingleAsync();
        Assert.Equal(4m, updatedShortage.ResolvedQty);
        Assert.Equal(6m, updatedShortage.OpenQty);
        Assert.Equal(100m, updatedShortage.ShortageValue);
        Assert.Equal(40m, updatedShortage.ResolvedAmount);
        Assert.Equal(60m, updatedShortage.OpenAmount);
        Assert.Equal(ShortageEntryStatus.PartiallyResolved, updatedShortage.Status);

        var statementEntry = await dbContext.SupplierStatementEntries.SingleAsync();
        Assert.Equal(SupplierStatementEffectType.ShortageFinancialResolution, statementEntry.EffectType);
        Assert.Equal(-40m, statementEntry.AmountDelta);
        Assert.Equal(-40m, statementEntry.RunningBalance);
    }

    [Fact]
    public async Task PostAsync_ShouldRejectPhysicalAllocationBeyondOpenQty()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 2m);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedQty: 3m)
            });

        var service = CreatePostingService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostAsync(resolution.Id, "tester", CancellationToken.None));

        Assert.Contains("Allocated quantity cannot exceed the open shortage quantity", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostAsync_ShouldRejectFinancialAllocationBeyondOpenAmount()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(
            dbContext,
            references,
            shortageQty: 5m,
            affectsSupplierBalance: true,
            shortageValue: 50m,
            resolvedAmount: 20m,
            resolvedQty: 2m);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Financial,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedAmount: 40m)
            });

        var service = CreatePostingService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostAsync(resolution.Id, "tester", CancellationToken.None));

        Assert.Contains("Allocated amount cannot exceed the open shortage amount", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostAsync_ShouldAllowOneShortageRowAcrossMultipleResolutions()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 10m, affectsSupplierBalance: true);
        var firstResolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedQty: 4m)
            },
            resolutionNo: "SR-STEP-1");
        var secondResolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Financial,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedAmount: 60m, ValuationRate: 10m)
            },
            resolutionNo: "SR-STEP-2");

        var service = CreatePostingService(dbContext);

        await service.PostAsync(firstResolution.Id, "tester", CancellationToken.None);
        await service.PostAsync(secondResolution.Id, "tester", CancellationToken.None);

        var updatedShortage = await dbContext.ShortageLedgerEntries.SingleAsync();
        Assert.Equal(10m, updatedShortage.ResolvedQty);
        Assert.Equal(0m, updatedShortage.OpenQty);
        Assert.Equal(100m, updatedShortage.ShortageValue);
        Assert.Equal(100m, updatedShortage.ResolvedAmount);
        Assert.Equal(0m, updatedShortage.OpenAmount);
        Assert.Equal(ShortageEntryStatus.Resolved, updatedShortage.Status);
    }

    [Fact]
    public async Task PostAsync_ShouldBeIdempotentWhenCalledTwice()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        var shortage = await CreateShortageAsync(dbContext, references, shortageQty: 4m);
        var resolution = await CreateResolutionAsync(
            dbContext,
            references.Supplier,
            ShortageResolutionType.Physical,
            new[]
            {
                new AllocationSeed(shortage.Id, AllocatedQty: 4m)
            });

        var service = CreatePostingService(dbContext);

        var first = await service.PostAsync(resolution.Id, "tester", CancellationToken.None);
        var second = await service.PostAsync(resolution.Id, "tester", CancellationToken.None);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(DocumentStatus.Posted, second!.Status);
        Assert.Equal(1, await dbContext.StockLedgerEntries.CountAsync());
        Assert.Equal(0, await dbContext.SupplierStatementEntries.CountAsync());
    }

    [Fact]
    public async Task ListOpenShortagesAsync_ShouldIncludeLegacyRowsWithMissingOpenQty()
    {
        await using var dbContext = CreateDbContext();
        var references = await SeedShortageReferencesAsync(dbContext);
        await CreateShortageAsync(dbContext, references, shortageQty: 7m);

        var legacyRow = await dbContext.ShortageLedgerEntries.SingleAsync();
        legacyRow.OpenQty = 0m;
        legacyRow.OpenAmount = null;
        await dbContext.SaveChangesAsync();

        var service = new ShortageResolutionService(dbContext);

        var result = await service.ListOpenShortagesAsync(
            new OpenShortageQuery(null, references.Supplier.Id, null, null, null, null, null, null),
            CancellationToken.None);

        var shortage = Assert.Single(result);
        Assert.Equal(7m, shortage.OpenQty);
        Assert.Equal(ShortageEntryStatus.Open, shortage.Status);
    }

    private static IShortageResolutionPostingService CreatePostingService(AppDbContext dbContext)
    {
        var resolutionService = new ShortageResolutionService(dbContext);
        var validationService = new ShortageResolutionValidationService(dbContext);
        var allocationService = new ShortageResolutionAllocationService(dbContext);

        return new ShortageResolutionPostingService(dbContext, resolutionService, validationService, allocationService);
    }

    private static AppDbContext CreateDbContext()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"highcool-shortage-resolution-tests-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private static async Task<ShortageReferences> SeedShortageReferencesAsync(AppDbContext dbContext)
    {
        var supplier = new Supplier
        {
            Code = "SUP-SHORT",
            Name = "Shortage Supplier",
            StatementName = "Shortage Supplier",
            IsActive = true,
            CreatedBy = "seed"
        };

        var warehouse = new Warehouse
        {
            Code = "MAIN",
            Name = "Main Warehouse",
            IsActive = true,
            CreatedBy = "seed"
        };

        var pieceUom = new Uom
        {
            Code = "PCS",
            Name = "Pieces",
            Precision = 0,
            AllowsFraction = false,
            IsActive = true,
            CreatedBy = "seed"
        };

        var parentItem = new Item
        {
            Code = "ITM-SHORT",
            Name = "Shortage Parent",
            BaseUomId = Guid.Empty,
            IsActive = true,
            IsSellable = true,
            HasComponents = true,
            CreatedBy = "seed"
        };

        var componentItem = new Item
        {
            Code = "CMP-SHORT",
            Name = "Shortage Component",
            BaseUomId = Guid.Empty,
            IsActive = true,
            IsSellable = false,
            HasComponents = false,
            CreatedBy = "seed"
        };

        dbContext.Suppliers.Add(supplier);
        dbContext.Warehouses.Add(warehouse);
        dbContext.Uoms.Add(pieceUom);
        await dbContext.SaveChangesAsync();

        parentItem.BaseUomId = pieceUom.Id;
        componentItem.BaseUomId = pieceUom.Id;
        dbContext.Items.AddRange(parentItem, componentItem);
        await dbContext.SaveChangesAsync();

        return new ShortageReferences(supplier, warehouse, pieceUom, parentItem, componentItem);
    }

    private static async Task<ShortageLedgerEntry> CreateShortageAsync(
        AppDbContext dbContext,
        ShortageReferences references,
        decimal shortageQty,
        bool affectsSupplierBalance = false,
        decimal? shortageValue = null,
        decimal resolvedAmount = 0m,
        decimal resolvedQty = 0m,
        string suffix = "0001")
    {
        var receipt = new PurchaseReceipt
        {
            ReceiptNo = $"PR-SHORT-{suffix}",
            SupplierId = references.Supplier.Id,
            WarehouseId = references.Warehouse.Id,
            ReceiptDate = new DateTime(2026, 4, 21),
            Status = DocumentStatus.Posted,
            CreatedBy = "seed"
        };

        dbContext.PurchaseReceipts.Add(receipt);
        await dbContext.SaveChangesAsync();

        var receiptLine = new PurchaseReceiptLine
        {
            PurchaseReceiptId = receipt.Id,
            LineNo = 1,
            ItemId = references.ParentItem.Id,
            ReceivedQty = 1m,
            UomId = references.BaseUom.Id,
            CreatedBy = "seed"
        };

        dbContext.PurchaseReceiptLines.Add(receiptLine);
        await dbContext.SaveChangesAsync();

        var entry = new ShortageLedgerEntry
        {
            PurchaseReceiptId = receipt.Id,
            PurchaseReceiptLineId = receiptLine.Id,
            ItemId = references.ParentItem.Id,
            ComponentItemId = references.ComponentItem.Id,
            ExpectedQty = shortageQty,
            ActualQty = 0m,
            ShortageQty = shortageQty,
            ResolvedQty = resolvedQty,
            OpenQty = shortageQty - resolvedQty,
            ShortageValue = shortageValue,
            ResolvedAmount = resolvedAmount,
            OpenAmount = shortageValue.HasValue ? shortageValue.Value - resolvedAmount : null,
            AffectsSupplierBalance = affectsSupplierBalance,
            ApprovalStatus = "NotRequired",
            Status = resolvedQty == 0m ? ShortageEntryStatus.Open : ShortageEntryStatus.PartiallyResolved,
            CreatedBy = "seed"
        };

        dbContext.ShortageLedgerEntries.Add(entry);
        await dbContext.SaveChangesAsync();
        return entry;
    }

    private static async Task<ShortageResolution> CreateResolutionAsync(
        AppDbContext dbContext,
        Supplier supplier,
        ShortageResolutionType type,
        IReadOnlyList<AllocationSeed> allocations,
        string resolutionNo = "SR-TEST-0001")
    {
        var resolution = new ShortageResolution
        {
            ResolutionNo = resolutionNo,
            SupplierId = supplier.Id,
            ResolutionType = type,
            ResolutionDate = new DateTime(2026, 4, 21),
            TotalQty = type == ShortageResolutionType.Physical ? allocations.Sum(entity => entity.AllocatedQty ?? 0m) : null,
            TotalAmount = type == ShortageResolutionType.Financial ? allocations.Sum(entity => entity.AllocatedAmount ?? 0m) : null,
            Currency = "EGP",
            Notes = "Resolution",
            Status = DocumentStatus.Draft,
            CreatedBy = "seed"
        };

        foreach (var allocation in allocations.Select((value, index) => (value, index)))
        {
            resolution.Allocations.Add(new ShortageResolutionAllocation
            {
                ShortageLedgerId = allocation.value.ShortageLedgerId,
                AllocatedQty = allocation.value.AllocatedQty,
                AllocatedAmount = allocation.value.AllocatedAmount,
                ValuationRate = allocation.value.ValuationRate,
                AllocationMethod = "Manual",
                SequenceNo = allocation.index + 1,
                CreatedBy = "seed"
            });
        }

        dbContext.ShortageResolutions.Add(resolution);
        await dbContext.SaveChangesAsync();
        return resolution;
    }

    private sealed record ShortageReferences(
        Supplier Supplier,
        Warehouse Warehouse,
        Uom BaseUom,
        Item ParentItem,
        Item ComponentItem);

    private sealed record AllocationSeed(
        Guid ShortageLedgerId,
        decimal? AllocatedQty = null,
        decimal? AllocatedAmount = null,
        decimal? ValuationRate = null);
}
