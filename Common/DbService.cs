using System;
using Common.Interfaces;
using Common.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Common
{
    public class DbService : IDbService
    {
        private readonly ISecretsService secretsService;
        private readonly IEnvironmentVariableService environmentVariableService;

        private const string USERS_CONTAINER_NAME = "users";

        private CosmosClient? client;

        public DbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
        {
            this.secretsService = secretsService;
            this.environmentVariableService = environmentVariableService;
        }

        private async Task<CosmosClient> GetClient()
        {
            if (this.client == null)
            {
                string endpoint = this.environmentVariableService.GetVariable(EnvironmentVariableKeys.CosmosDbEndpoint);
                string key = await this.secretsService.GetSecret(SecretsKeys.CosmosClient);

                var client = new CosmosClient(endpoint, key);
                this.client = client;
            }

            return this.client;
        }

        private async Task<Database> GetTradingDb()
        {
            var client = await GetClient();

            return client.GetDatabase("trading");
        }

        private async Task<Container> GetTradingContainer(string containerName)
        {
            var database = await GetTradingDb();

            return database.GetContainer(containerName);
        }

        public async Task<Trader> GetUser(string azureUserId)
        {
            var usersContainer = await this.GetTradingContainer(USERS_CONTAINER_NAME);

            var itemResponse = await usersContainer.ReadItemAsync<Trader>(azureUserId, new PartitionKey(azureUserId));

            if (itemResponse == null)
            {
                throw new MissingItemResponse(azureUserId, USERS_CONTAINER_NAME);
            }

            return itemResponse.Resource;
        }
    }
}

