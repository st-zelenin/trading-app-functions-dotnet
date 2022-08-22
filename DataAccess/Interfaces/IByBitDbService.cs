using System;
using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface IByBitDbService
    {
        Task<IEnumerable<ByBitAverage>> GetAveragesAsync(string containerId);
        Task<IEnumerable<ByBitOrder>> GetFilledOrdersAsync(string pair, string containerId);
        Task<IEnumerable<ByBitOrder>> GetOrdersAsync(string pair, string containerId);
        Task<IEnumerable<ByBitOrder>> GetOrdersBySide(string side, int limit, string containerId);
        Task UpsertOrdersAsync(IEnumerable<ByBitOrder> orders, string containerId);
    }
}

