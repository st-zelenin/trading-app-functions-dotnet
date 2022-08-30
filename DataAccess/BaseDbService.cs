using Common;
using Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class BaseDbService
    {
        protected readonly string dbName;

        private readonly ISecretsService secretsService;
        private readonly IEnvironmentVariableService environmentVariableService;

        private CosmosClient? client;
        private Database? database;

        public BaseDbService(string dbName, ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
        {
            this.dbName = dbName;
            this.secretsService = secretsService;
            this.environmentVariableService = environmentVariableService;
        }

        private async Task<CosmosClient> GetClientAsync()
        {
            if (this.client == null)
            {
                string endpoint = this.environmentVariableService.GetVariable(EnvironmentVariableKeys.CosmosDbEndpoint);
                string key = await this.secretsService.GetSecretAsync(SecretsKeys.CosmosClient);

                var client = new CosmosClient(endpoint, key, new CosmosClientOptions() { AllowBulkExecution = true });
                this.client = client;
            }

            return this.client;
        }

        protected async Task<Database> GetDatabaseAsync()
        {
            if (this.database == null)
            {
                var client = await GetClientAsync();
                var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(this.dbName);

                //return databaseResponse.Database;
                this.database = databaseResponse.Database;
                //this.database = client.GetDatabase(this.dbName);
            }

            return this.database;
        }

        protected Task<IEnumerable<T>> ExecuteReadQueryAsync<T>(string query, Container container)
        {
            return this.ExecuteReadQueryAsync<T>(new QueryDefinition(query), container);
        }

        protected async Task<IEnumerable<T>> ExecuteReadQueryAsync<T>(QueryDefinition query, Container container)
        {
            var result = new List<T>();

            using (var feed = container.GetItemQueryIterator<T>(query))
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
    }
}

