namespace ERP.Application.Inventory;

public interface IStockBalanceService
{
    Task<IReadOnlyList<StockBalanceDto>> ListAsync(StockBalanceQuery query, CancellationToken cancellationToken);
}
