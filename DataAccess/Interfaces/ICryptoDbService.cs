using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface ICryptoDbService
{
    Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId);
    Task<IEnumerable<CryptoOrder>> GetFilledOrdersAsync(string pair, string containerId);
    Task<IEnumerable<CryptoOrder>> GetOrdersAsync(string pair, string containerId);
    Task<IEnumerable<CryptoOrder>> GetOrdersBySide(string side, int limit, string containerId);
    Task UpsertOrdersAsync(IEnumerable<CryptoOrder> orders, string containerId);

    Task DoSomeTechService(string containerId);
}


