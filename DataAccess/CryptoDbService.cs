using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class CryptoDbService: BaseDbService, ICryptoDbService
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
            var result = new List<CryptoAverage>();

            using (var feed = container.GetItemQueryIterator<CryptoAverage>(query))
            {
                while (feed.HasMoreResults)
                {
                    foreach (var average in await feed.ReadNextAsync())
                    {
                        result.Add(average);
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<CryptoOrder>> GetFilledOrdersAsync(string pair, string containerId)
        {
            var database = await this.GetDatabaseAsync();
            var container = database.GetContainer(containerId);


            var query = new QueryDefinition("SELECT * FROM c WHERE c.instrument_name = @pair AND c.status = \"FILLED\" ORDER BY c.update_time DESC")
                .WithParameter("@pair", pair);

            var result = new List<CryptoOrder>();

            using (var feed = container.GetItemQueryIterator<CryptoOrder>(query))
            {
                while (feed.HasMoreResults)
                {
                    foreach (var order in await feed.ReadNextAsync())
                    {
                        result.Add(order);
                    }
                }
            }

            return result;
        }
    }
}

