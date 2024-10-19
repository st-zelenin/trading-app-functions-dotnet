using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class BinanceDbService : BaseDbService, IBinanceDbService
    {
        public BinanceDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
            : base("binance", secretsService, environmentVariableService)
        {
        }

        public async Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId)
        {
            var query =
                "SELECT SUM(StringToNumber(c.cummulativeQuoteQty)) AS total_money, SUM(StringToNumber(c.executedQty)) AS total_volume, c.side, c.symbol AS currency_pair FROM c WHERE c.status = 'FILLED' GROUP BY c.side, c.symbol";

            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            return await this.ExecuteReadQueryAsync<CryptoAverage>(query, container);
        }

        public Task<IEnumerable<BinanceOrder>> GetFilledOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @pair AND c.status = \"FILLED\" ORDER BY c.updateTime DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<BinanceOrder>> GetOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @pair ORDER BY c.updateTime DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<BinanceOrder>> GetOrdersBySide(string side, int limit, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.status = \"FILLED\" AND c.side = @side ORDER BY c.updateTime DESC OFFSET 0 LIMIT @limit")
                .WithParameter("@side", side)
                .WithParameter("@limit", limit);

            return this.QueryOrdersAsync(containerId, query);
        }

        public async Task UpsertOrdersAsync(IEnumerable<BinanceOrder> orders, string containerId)
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


        private async Task<IEnumerable<BinanceOrder>> QueryOrdersAsync(string containerId, QueryDefinition query)
        {
            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            return await this.ExecuteReadQueryAsync<BinanceOrder>(query, container);
        }
    }
}

