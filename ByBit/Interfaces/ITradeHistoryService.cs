using System;
using System.Threading.Tasks;

namespace ByBit.Interfaces
{
    public interface ITradeHistoryService
    {
        Task UpdateRecentTradeHistoryAsync(string pair, string azureUserId);
        Task ImportTradeHistoryAsync(string pair, string azureUserId);
    }
}

