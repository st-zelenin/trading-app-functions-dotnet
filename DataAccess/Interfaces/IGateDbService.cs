using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces;


public interface IGateDbService
{
    Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId);
    Task<IEnumerable<GateOrder>> GetFilledOrdersAsync(string pair, string containerId);
    Task<IEnumerable<GateOrder>> GetOrdersAsync(string pair, string containerId);
    Task<IEnumerable<GateOrder>> GetOrdersBySide(string side, int limit, string containerId);
    Task UpsertOrdersAsync(IEnumerable<GateOrder> orders, string containerId);
    Task DoSomeTechService(string containerId);
}
