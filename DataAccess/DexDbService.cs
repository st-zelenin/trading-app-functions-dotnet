using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess;

public class DexDbService : BaseDbService, IDexDbService
{
    public DexDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
            : base("dex", secretsService, environmentVariableService)
    {
    }

    public async Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId, string associatedCex)
    {
        var container = await GetContainerAsync(containerId);

        var query =
            "SELECT SUM(c.amount * c.price) AS total_money, SUM(c.amount) AS total_volume, c.side, c.currencyPair AS currency_pair FROM c GROUP BY c.side, c.currencyPair";

        return await this.ExecuteReadQueryAsync<CryptoAverage>(query, container);
    }

    public Task<IEnumerable<Order>> GetOrdersAsync(string pair, string containerId, string associatedCex)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.currencyPair = @pair AND c.associatedCex = @associatedCex ORDER BY c.updateTimestamp DESC")
            .WithParameter("@pair", pair)
            .WithParameter("@associatedCex", associatedCex);


        return this.QueryOrdersAsync(containerId, query);
    }

    public Task<IEnumerable<Order>> GetOrdersBySide(string side, string containerId, string associatedCex)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.side = @side AND c.associatedCex = @associatedCex ORDER BY c.updateTimestamp DESC")
            .WithParameter("@side", side)
            .WithParameter("@associatedCex", associatedCex);

        return this.QueryOrdersAsync(containerId, query);
    }

    private async Task<IEnumerable<Order>> QueryOrdersAsync(string containerId, QueryDefinition query)
    {
        var container = await GetContainerAsync(containerId);

        return await this.ExecuteReadQueryAsync<Order>(query, container);
    }

    public async Task<Order> UpsertOrderAsync(Order order, string containerId)
    {
        var container = await GetContainerAsync(containerId);

        var result = await container.Container.UpsertItemAsync(order);

        return result.Resource;
    }

    private async Task<ContainerResponse> GetContainerAsync(string containerId)
    {
        var database = await this.GetDatabaseAsync();

        return await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
        {
            Id = containerId,
            PartitionKeyPath = "/currencyPair"
        });
    }
}
