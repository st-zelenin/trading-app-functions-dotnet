using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess;

public class ByBitDbService : BaseDbService, IByBitDbService
{
    public ByBitDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
        : base("bybit", secretsService, environmentVariableService)
    {
    }

    public async Task<IEnumerable<ByBitAverage>> GetAveragesAsync(string containerId)
    {
        var query =
          "SELECT SUM(c.type = 'MARKET' ? (StringToNumber(c.executedQty) * StringToNumber(c.avgPrice)) : StringToNumber(c.cummulativeQuoteQty)) AS total_money, SUM(StringToNumber(c.executedQty)) AS total_volume, c.side, c.symbol AS currency_pair FROM c WHERE c.status = 'FILLED' OR c.status = 'PARTIALLY_FILLED' GROUP BY c.side, c.symbol";

        var database = await this.GetDatabaseAsync();
        var container = database.GetContainer(containerId);

        return await this.ExecuteReadQueryAsync<ByBitAverage>(query, container);
    }

    public Task<IEnumerable<ByBitOrder>> GetFilledOrdersAsync(string pair, string containerId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @pair AND c.status = \"FILLED\" ORDER BY c.updateTime DESC")
            .WithParameter("@pair", pair);

        return this.QueryOrdersAsync(containerId, query);
    }

    public Task<IEnumerable<ByBitOrder>> GetOrdersAsync(string pair, string containerId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @pair ORDER BY c.updateTime DESC")
            .WithParameter("@pair", pair);

        return this.QueryOrdersAsync(containerId, query);
    }

    public Task<IEnumerable<ByBitOrder>> GetOrdersBySide(string side, int limit, string containerId)
    {
        // TODO: remove UPPER
        var query = new QueryDefinition("SELECT * FROM c WHERE c.status = \"FILLED\" AND UPPER(c.side) = @side ORDER BY c.updateTime DESC OFFSET 0 LIMIT @limit")
            .WithParameter("@side", side)
            .WithParameter("@limit", limit);

        return this.QueryOrdersAsync(containerId, query);
    }

    private async Task<IEnumerable<ByBitOrder>> QueryOrdersAsync(string containerId, QueryDefinition query)
    {
        var database = await this.GetDatabaseAsync();
        var container = database.GetContainer(containerId);

        return await this.ExecuteReadQueryAsync<ByBitOrder>(query, container);
    }

    public async Task UpsertOrdersAsync(IEnumerable<ByBitOrder> orders, string containerId)
    {
        var database = await this.GetDatabaseAsync();
        var container = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
        {
            Id = containerId,
            PartitionKeyPath = "/symbol"
        });

        var tasks = new List<Task>();
        foreach (var order in orders)
        {
            tasks.Add(container.Container.UpsertItemAsync(order));
        }

        await Task.WhenAll(tasks);
    }
}

