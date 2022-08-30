using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface ITradingDbService
    {
        Task<Trader> GetUserAsync(string azureUserId);
        Task<Trader> GetOrCreateUserAsync(AzureUser azureUser);
        Task<Trader> UpdateUserAsync(Trader user);
        Task<IEnumerable<Trader>> GetUsersAsync();
    }
}

