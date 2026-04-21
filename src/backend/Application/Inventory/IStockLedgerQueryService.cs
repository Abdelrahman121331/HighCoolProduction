namespace ERP.Application.Inventory;

public interface IStockLedgerQueryService
{
    Task<IReadOnlyList<StockLedgerEntryDto>> ListAsync(StockLedgerQuery query, CancellationToken cancellationToken);
}
