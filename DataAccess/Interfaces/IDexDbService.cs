using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IDexDbService
{
    Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId, string associatedCex);
    Task<IEnumerable<Order>> GetOrdersAsync(string pair, string containerId, string associatedCex);
    Task<IEnumerable<Order>> GetOrdersBySide(string side, string containerId, string associatedCex);
    Task<Order> UpsertOrderAsync(Order order, string containerId);
}
