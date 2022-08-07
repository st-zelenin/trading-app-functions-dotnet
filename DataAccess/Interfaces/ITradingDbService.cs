using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface ITradingDbService
    {
        Task<Trader> GetUserAsync(string azureUserId);
    }
}

