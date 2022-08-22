using System;
using System.Threading.Tasks;

namespace Gate.Interfaces
{
    public interface ITradeHistoryService
    {
        int historyDaysDiff { get; }
        Task UpdateRecentTradeHistory(string pair, string azureUserId);
        Task ImportPeriodTradeHistory(string pair, DateTime end, DateTime start, string azureUserId);
        Task ImportTradeHistoryAsync(string pair, string azureUserId);
    }
}

