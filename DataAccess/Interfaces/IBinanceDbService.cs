using System;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IBinanceDbService
{
    Task<IEnumerable<BinanceAverage>> GetAveragesAsync(string containerId);
    Task<IEnumerable<BinanceOrder>> GetFilledOrdersAsync(string pair, string containerId);
    Task<IEnumerable<BinanceOrder>> GetOrdersAsync(string pair, string containerId);
    Task<IEnumerable<BinanceOrder>> GetOrdersBySide(string side, int limit, string containerId);
    Task UpsertOrdersAsync(IEnumerable<BinanceOrder> orders, string containerId);
}
