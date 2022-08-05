using System;
using Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Common
{
    public class DbService : IDbService
    {
        private ISecretsService secretsService;
        private IEnvironmentVariableService environmentVariableService;

        public DbService(ISecretsService secretsService, IEnvironmentVariableService environmentVariableService)
        {
            this.secretsService = secretsService;
            this.environmentVariableService = environmentVariableService;
        }

        private CosmosClient GetClient()
        {
            string endpoint = this.environmentVariableService.GetVariable(EnvironmentVariableKeys.CosmosDbEndpoint);
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

