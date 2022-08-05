using System;
using Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Common
{
    public class DbService
    {
        private ISecretsService secretsService;

        public DbService(ISecretsService secretsService)
        {
            this.secretsService = secretsService;
        }

        private CosmosClient GetClient()
        {
            string endpoint = EnvironmentVariableService.GetVariable(EnvironmentVariableService.Keys.CosmosDbEndpoint);
            string key = this.secretsService.GetSecret(SecretsKeys.CosmosClient);

            return new CosmosClient(endpoint, key);
        }

        public Database GetTradingDb()
        {
            return GetClient().GetDatabase("trading");
        }

        public Container GetUsersContainer()
        {
            return GetTradingDb().GetContainer("users");
        }
    }
}

