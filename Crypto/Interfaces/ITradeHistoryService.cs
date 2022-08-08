using System;
using System.Threading.Tasks;

namespace Crypto.Interfaces
{
    public interface ITradeHistoryService
    {
        Task UpdateRecentTradeHistory(string azureUserId);
    }
}

