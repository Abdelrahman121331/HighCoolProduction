using ERP.Domain.MasterData;
using ERP.Domain.Shortages;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ERP.Infrastructure;

public sealed class DevelopmentDatabaseInitializer(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";

        if (!hostEnvironment.IsDevelopment() ||
            !string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        await SeedAsync(dbContext, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Uoms.AnyAsync(cancellationToken))
        {
            return;
        }

        var pieceUom = new Uom
        {
            Code = "PCS",
            Name = "Pieces",
            Precision = 0,
            AllowsFraction = false,
            IsActive = true,
            CreatedBy = "system"
        };

        var kilogramUom = new Uom
        {
            Code = "KG",
            Name = "Kilogram",
            Precision = 3,
            AllowsFraction = true,
            IsActive = true,
            CreatedBy = "system"
        };

        var mainWarehouse = new Warehouse
        {
            Code = "MAIN",
            Name = "Main Warehouse",
            Location = "Head Office",
            IsActive = true,
            CreatedBy = "system"
        };

        var outletWarehouse = new Warehouse
        {
            Code = "OUTLET",
            Name = "Outlet Warehouse",
            Location = "Retail Branch",
            IsActive = true,
            CreatedBy = "system"
        };

        var supplierA = new Supplier
        {
            Code = "SUP-001",
            Name = "Delta Cooling Supplies",
            StatementName = "Delta Cooling Supplies",
            Phone = "+20-100-000-0001",
            Email = "accounts@deltacooling.example",
            IsActive = true,
            CreatedBy = "system"
        };

        var supplierB = new Supplier
        {
            Code = "SUP-002",
            Name = "Nile Components Trading",
            StatementName = "Nile Components Trading",
            Phone = "+20-100-000-0002",
            Email = "sales@nilecomponents.example",
            IsActive = true,
            CreatedBy = "system"
        };

        var fanMotor = new Item
        {
            Code = "ITM-001",
            Name = "Fan Motor",
            BaseUomId = pieceUom.Id,
            IsActive = true,
            IsSellable = true,
            HasComponents = false,
            CreatedBy = "system"
        };

        var copperCoil = new Item
        {
            Code = "ITM-002",
            Name = "Copper Coil",
            BaseUomId = kilogramUom.Id,
            IsActive = true,
            IsSellable = false,
            HasComponents = false,
            CreatedBy = "system"
        };

        var coolingUnit = new Item
        {
            Code = "ITM-003",
            Name = "Cooling Unit",
            BaseUomId = pieceUom.Id,
            IsActive = true,
            IsSellable = true,
            HasComponents = true,
            CreatedBy = "system"
        };

        var itemComponent = new ItemComponent
        {
            ItemId = coolingUnit.Id,
            ComponentItemId = fanMotor.Id,
            UomId = pieceUom.Id,
            Quantity = 1m,
            CreatedBy = "system"
        };

        var itemConversion = new UomConversion
        {
            FromUomId = pieceUom.Id,
            ToUomId = kilogramUom.Id,
            Factor = 0.25m,
            RoundingMode = RoundingMode.Round,
            IsActive = true,
            CreatedBy = "system"
        };

        var transitShortageReason = new ShortageReasonCode
        {
            Code = "TRANSIT_SHORTAGE",
            Name = "Transit shortage",
            Description = "Quantity was short during receipt capture and needs investigation.",
            AffectsSupplierBalance = false,
            AffectsStock = false,
            IsActive = true,
            CreatedBy = "system"
        };

        var supplierShortageReason = new ShortageReasonCode
        {
            Code = "SUPPLIER_SHORT",
            Name = "Supplier short supply",
            Description = "Supplier delivered less than expected and the shortage should affect supplier follow-up.",
            AffectsSupplierBalance = true,
            AffectsStock = false,
            IsActive = true,
            CreatedBy = "system"
        };

        dbContext.Uoms.AddRange(pieceUom, kilogramUom);
        dbContext.Warehouses.AddRange(mainWarehouse, outletWarehouse);
        dbContext.Suppliers.AddRange(supplierA, supplierB);
        dbContext.Items.AddRange(fanMotor, copperCoil, coolingUnit);
        dbContext.ItemComponents.Add(itemComponent);
        dbContext.UomConversions.Add(itemConversion);
        dbContext.ShortageReasonCodes.AddRange(transitShortageReason, supplierShortageReason);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
