using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IBinanceDbService
{
    Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId);
    Task<IEnumerable<BinanceOrder>> GetFilledOrdersAsync(string pair, string containerId);
    Task<IEnumerable<BinanceOrder>> GetOrdersAsync(string pair, string containerId);
    Task<IEnumerable<BinanceOrder>> GetOrdersBySide(string side, int limit, string containerId);
    Task UpsertOrdersAsync(IEnumerable<BinanceOrder> orders, string containerId);
}
