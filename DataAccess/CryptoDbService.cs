using System.ComponentModel;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class CryptoDbService : BaseDbService, ICryptoDbService
    {
        public CryptoDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
            : base("crypto", secretsService, environmentVariableService)
        {
        }

        public async Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId)
        {
            var query =
                "SELECT SUM(c.cumulative_value) AS total_money, SUM(c.cumulative_quantity) AS total_volume, c.side, c.instrument_name AS currency_pair FROM c WHERE c.status = 'FILLED' GROUP BY c.side, c.instrument_name";

            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            return await this.ExecuteReadQueryAsync<CryptoAverage>(query, container);
        }

        public Task<IEnumerable<CryptoOrder>> GetFilledOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.instrument_name = @pair AND c.status = \"FILLED\" ORDER BY c.update_time DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<CryptoOrder>> GetOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.instrument_name = @pair ORDER BY c.update_time DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<CryptoOrder>> GetOrdersBySide(string side, int limit, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.status = \"FILLED\" AND c.side = @side ORDER BY c.update_time DESC OFFSET 0 LIMIT @limit")
                .WithParameter("@side", side)
                .WithParameter("@limit", limit);

            return this.QueryOrdersAsync(containerId, query);
        }

        private async Task<IEnumerable<CryptoOrder>> QueryOrdersAsync(string containerId, QueryDefinition query)
        {
            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            return await this.ExecuteReadQueryAsync<CryptoOrder>(query, container);
        }

        public async Task UpsertOrdersAsync(IEnumerable<CryptoOrder> orders, string containerId)
        {
            var database = await this.GetDatabaseAsync();
            var container = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
            {
                Id = containerId,
                PartitionKeyPath = "/instrument_name"
            });

            var tasks = new List<Task>();
            foreach (var order in orders)
            {
                tasks.Add(container.Container.UpsertItemAsync(order));
            }

            await Task.WhenAll(tasks);
        }

        public async Task DoSomeTechService(string containerId)
        {
            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            var query = new QueryDefinition("select * from c where endswith(c.instrument_name, \"USDC\")");
            var results = await this.ExecuteReadQueryAsync<CryptoOrder>(query, container);

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
            //}
        }
    }
}

