using Common.Models;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface ITradingDbService
{
    Task<Trader> GetUserAsync(string azureUserId);
    Task<Trader> GetOrCreateUserAsync(AzureUser azureUser);
    Task<Trader> UpdateUserAsync(Trader user);
    Task<IEnumerable<Trader>> GetUsersAsync();
    Task<Trader> AddPairAsync(string azureUserId, ExchangeSymbol exchangeSymbol);
    Task<Trader> RemovePairAsync(string azureUserId, ExchangeSymbol exchangeSymbol);
    Task<Trader> OrderPairsAsync(string azureUserId, OrderedSymbols orderedSymbols);
    Task<Trader> TogglePairArchiveAsync(string azureUserId, ExchangeSymbol exchangeSymbol);
    Task DoSomeTechService(string azureUserId);
}
