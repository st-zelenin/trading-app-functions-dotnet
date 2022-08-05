using System;
using Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Common
{
    public class DbService : IDbService
    {
        private readonly ISecretsService secretsService;
        private readonly IEnvironmentVariableService environmentVariableService;

        private CosmosClient client;

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

        public async Task<Database> GetTradingDb()
        {
            var client = await GetClient();

            return client.GetDatabase("trading");
        }

        public async Task<Container> GetUsersContainer()
        {
            var database = await GetTradingDb();

            return database.GetContainer("users");
        }
    }
}

