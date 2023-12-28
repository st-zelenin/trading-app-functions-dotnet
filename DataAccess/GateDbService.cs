using System;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class GateDbService : BaseDbService, IGateDbService
    {
        public GateDbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
       : base("gate", secretsService, environmentVariableService)
        {
        }

        public async Task<IEnumerable<GateAverage>> GetAveragesAsync(string containerId)
        {
            var query =
                "SELECT SUM(StringToNumber(c.filled_total)) AS total_money, SUM(is_defined(c.avg_deal_price) AND c.avg_deal_price != null ? StringToNumber(c.filled_total)/StringToNumber(c.avg_deal_price) : StringToNumber(c.amount)) AS total_volume, c.side, c.currency_pair FROM c WHERE c.status = 'closed' GROUP BY c.side, c.currency_pair";

            var container = await this.GetContainerAsync(containerId);

            return await this.ExecuteReadQueryAsync<GateAverage>(query, container);
        }

        public Task<IEnumerable<GateOrder>> GetFilledOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.currency_pair = @pair AND c.status = \"closed\" ORDER BY c.update_time_ms DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<GateOrder>> GetOrdersAsync(string pair, string containerId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.currency_pair = @pair ORDER BY c.update_time_ms DESC")
                .WithParameter("@pair", pair);

            return this.QueryOrdersAsync(containerId, query);
        }

        public Task<IEnumerable<GateOrder>> GetOrdersBySide(string side, int limit, string containerId)
        {
            // TODO: remove UPPER
            var query = new QueryDefinition("SELECT * FROM c WHERE c.status = \"closed\" AND UPPER(c.side) = @side ORDER BY c.update_time DESC OFFSET 0 LIMIT @limit")
                .WithParameter("@side", side)
                .WithParameter("@limit", limit);

            return this.QueryOrdersAsync(containerId, query);
        }

        private async Task<IEnumerable<GateOrder>> QueryOrdersAsync(string containerId, QueryDefinition query)
        {
            var container = await this.GetContainerAsync(containerId);

            return await this.ExecuteReadQueryAsync<GateOrder>(query, container);
        }

        public async Task UpsertOrdersAsync(IEnumerable<GateOrder> orders, string containerId)
        {
            var container = await GetContainerAsync(containerId);

            List<Task> tasks = new();
            foreach (var order in orders)
            {
                tasks.Add(container.UpsertItemAsync(order));
            }

            await Task.WhenAll(tasks);
        }

        public async Task DoSomeTechService(string containerId)
        {
            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.currency_pair = \"TONCOIN_USDT\"");
            var results = await this.ExecuteReadQueryAsync<GateOrder>(query, container);

            // foreach (var order in results)
            // {
            //    await container.DeleteItemAsync<GateOrder>(order.id, new PartitionKey(order.currency_pair));
            //    var orig_instrument_name = order.currency_pair;
            //    order.currency_pair = "TON_USDT";
            //    try
            //    {
            //        await container.UpsertItemAsync(partitionKey: new PartitionKey(order.currency_pair), item: order);
            //    }
            //    catch
            //    {
            //        order.currency_pair = orig_instrument_name;
            //        await container.UpsertItemAsync(partitionKey: new PartitionKey(order.currency_pair), item: order);
            //    }
            // }
        }

        private async Task<Container> GetContainerAsync(string containerId)
        {
            var database = await this.GetDatabaseAsync();

            var containerResponse = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
            {
                Id = containerId,
                PartitionKeyPath = "/currency_pair"
            });

            return containerResponse.Container;
        }
    }
}

