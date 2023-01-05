using System;
using System.Threading.Tasks;

namespace Binance.Interfaces;

public interface ITradeHistoryService
{
    Task UpdateRecentTradeHistoryAsync(string pair, string azureUserId);
    Task ImportTradeHistoryAsync(string pair, string azureUserId);
}

