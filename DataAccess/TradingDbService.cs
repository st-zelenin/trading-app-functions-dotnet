using System;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
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
                pairs = new List<string>(),
                crypto_pairs = new List<string>(),
                coinbase_pairs = new List<string>(),
                bybit_pairs = new List<string>()
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
    }
}

