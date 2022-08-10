using System;
using System.Threading.Tasks;

namespace Crypto.Interfaces
{
    public interface ITradeHistoryService
    {
        int historyHoursDiff { get; }
        Task UpdateRecentTradeHistory(string azureUserId);
        Task ImportPeriodTradeHistory(DateTime end, DateTime start, string azureUserId);
    }
}

