using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess;

public class TradingDbService : BaseDbService, ITradingDbService
{
    private Container? usersContainer;

    private const string USERS_CONTAINER_NAME = "users";

    public TradingDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
        : base("trading", secretsService, environmentVariableService)
    {
    }

    public async Task<Trader> GetUserAsync(string azureUserId)
    {
        if (this.usersContainer == null)
        {
            var database = await this.GetDatabaseAsync();
            this.usersContainer = database.GetContainer(USERS_CONTAINER_NAME);
        }

        var itemResponse = await usersContainer.ReadItemAsync<Trader>(azureUserId, new PartitionKey(azureUserId));

        return itemResponse.Resource;
    }

    public async Task<Trader> GetOrCreateUserAsync(AzureUser azureUser)
    {
        if (this.usersContainer == null)
        {
            var database = await this.GetDatabaseAsync();
            this.usersContainer = database.GetContainer(USERS_CONTAINER_NAME);
        }

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id OFFSET 0 LIMIT 1")
            .WithParameter("@id", azureUser.oid);

        var result = await this.ExecuteReadQueryAsync<Trader>(query, this.usersContainer);
        if (result.Count() > 0)
        {
            return result.First();
        }

        var response = await usersContainer.CreateItemAsync(new Trader()
        {
            id = azureUser.oid,
            name = azureUser.name,
            gate = new List<CryptoPair>(),
            crypto = new List<CryptoPair>(),
            coinbase = new List<CryptoPair>(),
            bybit = new List<CryptoPair>()
        });

        return response.Resource;
    }

    public async Task<IEnumerable<Trader>> GetUsersAsync()
    {
        if (this.usersContainer == null)
        {
            var database = await this.GetDatabaseAsync();
            this.usersContainer = database.GetContainer(USERS_CONTAINER_NAME);
        }

        return await this.ExecuteReadQueryAsync<Trader>("SELECT * FROM c", this.usersContainer);
    }

    public async Task<Trader> UpdateUserAsync(Trader user)
    {
        if (this.usersContainer == null)
        {
            var database = await this.GetDatabaseAsync();
            this.usersContainer = database.GetContainer(USERS_CONTAINER_NAME);
        }

        var response = await this.usersContainer.ReplaceItemAsync(user, user.id);

        return response.Resource;
    }

    public Task<Trader> AddPairAsync(string azureUserId, ExchangeSymbol exchangeSymbol)
    {
        return this.AddRemovePairAsync(azureUserId, exchangeSymbol, true);
    }

    public Task<Trader> RemovePairAsync(string azureUserId, ExchangeSymbol exchangeSymbol)
    {
        return this.AddRemovePairAsync(azureUserId, exchangeSymbol, false);
    }

    public async Task<Trader> OrderPairsAsync(string azureUserId, OrderedSymbols orderedSymbols)
    {
        var user = await this.GetUserAsync(azureUserId);
        if (user is null)
        {
            throw new Exception($"failed to get user with id: {azureUserId}");
        }

        var pairs = GetExchangePairs(orderedSymbols.exchange, user);
        if (pairs.Count != orderedSymbols.symbols.Count())
        {
            throw new Exception($"original and ordered pairs count is not the same: {pairs.Count} != {orderedSymbols.symbols.Count()}");
        }

        switch (orderedSymbols.exchange)
        {
            case Exchanges.GateIo:
                {
                    user.gate = this.OrderCryptoPairs(orderedSymbols.symbols, user.gate);
                    break;
                }
            case Exchanges.CryptoCom:
                {
                    user.crypto = this.OrderCryptoPairs(orderedSymbols.symbols, user.crypto);
                    break;
                }
            case Exchanges.Coinbase:
                {
                    user.coinbase = this.OrderCryptoPairs(orderedSymbols.symbols, user.coinbase);
                    break;
                }
            case Exchanges.ByBit:
                {
                    user.bybit = this.OrderCryptoPairs(orderedSymbols.symbols, user.bybit);
                    break;
                }
            case Exchanges.Binance:
                {
                    user.binance = this.OrderCryptoPairs(orderedSymbols.symbols, user.binance);
                    break;
                }
            default: throw new ArgumentException($"unhandled exchange: {orderedSymbols.exchange}");
        }

        var response = await this.usersContainer!.ReplaceItemAsync(user, user.id);
        return response.Resource;
    }

    public async Task<Trader> TogglePairArchiveAsync(string azureUserId, ExchangeSymbol exchangeSymbol)
    {
        var user = await this.GetUserAsync(azureUserId);
        if (user is null)
        {
            throw new Exception($"failed to get user with id: {azureUserId}");
        }

        var pairs = GetExchangePairs(exchangeSymbol.exchange, user);
        var pair = pairs.First(p => p.symbol == exchangeSymbol.symbol);
        pair.isArchived = !pair.isArchived;

        var response = await this.usersContainer!.ReplaceItemAsync(user, user.id);
        return response.Resource;
    }

    public async Task DoSomeTechService(string azureUserId)
    {
        var database = await this.GetDatabaseAsync();
        var container = database.GetContainer(USERS_CONTAINER_NAME);

        var query = new QueryDefinition("select * from c where c.id = @id")
            .WithParameter("@id", azureUserId);

        var results = await this.ExecuteReadQueryAsync<Trader>(query, container);

        var trader = results.First();

        // var updated = new Trader
        // {
        //     id = trader.id,
        //     name = trader.name,
        //     gate = trader.pairs.Select(p => new CryptoPair { symbol = p, isArchived = false }).ToList(),
        //     crypto = trader.crypto_pairs.Select(p => new CryptoPair { symbol = p, isArchived = false }).ToList(),
        //     coinbase = trader.coinbase_pairs.Select(p => new CryptoPair { symbol = p, isArchived = false }).ToList(),
        //     bybit = trader.bybit_pairs.Select(p => new CryptoPair { symbol = p, isArchived = false }).ToList(),
        //     binance = trader.binance_pairs.Select(p => new CryptoPair { symbol = p, isArchived = false }).ToList(),
        // };

        // await container.DeleteItemAsync<CryptoOrder>(trader.id, new PartitionKey(trader.id));
        // try
        // {
        //     await container.UpsertItemAsync(partitionKey: new PartitionKey(updated.id), item: updated);
        // }
        // catch
        // {
        //     await container.UpsertItemAsync(partitionKey: new PartitionKey(trader.id), item: trader);
        // }

        //foreach (var order in results)
        //{
        //    await container.DeleteItemAsync<CryptoOrder>(order.id, new PartitionKey(order.instrument_name));
        //    var orig_instrument_name = order.instrument_name;
        //    order.instrument_name = order.instrument_name.Replace("USDC", "USD");
        //    try
        //    {
        //        await container.UpsertItemAsync(partitionKey: new PartitionKey(order.instrument_name), item: order);
        //    }
        //    catch
        //    {
        //        order.instrument_name = orig_instrument_name;
        //        await container.UpsertItemAsync(partitionKey: new PartitionKey(order.instrument_name), item: order);
        //    }
        //}        }
    }

    private async Task<Trader> AddRemovePairAsync(string azureUserId, ExchangeSymbol exchangeSymbol, bool add)
    {
        var user = await this.GetUserAsync(azureUserId);
        if (user is null)
        {
            throw new Exception($"failed to get user with id: {azureUserId}");
        }

        var pairs = GetExchangePairs(exchangeSymbol.exchange, user);
        var pair = pairs.FirstOrDefault(p => p.symbol == exchangeSymbol.symbol);

        if (add)
        {
            if (pair is not null)
            {
                throw new Exception($"the pair {exchangeSymbol.symbol} was already added");
            }

            pairs.Add(new CryptoPair { symbol = exchangeSymbol.symbol, isArchived = false });
        }
        else
        {
            if (pair is null)
            {
                throw new Exception($"the pair {exchangeSymbol.symbol} was already removed");
            }

            pairs.Remove(pair);
        }

        var response = await this.usersContainer!.ReplaceItemAsync(user, user.id);
        return response.Resource;
    }

    private static IList<CryptoPair> GetExchangePairs(string exchange, Trader user)
    {
        return exchange switch
        {
            Exchanges.GateIo => user.gate,
            Exchanges.CryptoCom => user.crypto,
            Exchanges.Coinbase => user.coinbase,
            Exchanges.ByBit => user.bybit,
            Exchanges.Binance => user.binance,
            _ => throw new ArgumentException($"unhandled exchange: {exchange}"),
        };
    }

    private IList<CryptoPair> OrderCryptoPairs(IEnumerable<string> orderedSymbols, IList<CryptoPair> originalPairs)
    {
        var orderedPairs = new List<CryptoPair>();
        foreach (var symbol in orderedSymbols)
        {
            orderedPairs.Add(originalPairs.First(p => p.symbol == symbol));
        }

        return orderedPairs;
    }
}
