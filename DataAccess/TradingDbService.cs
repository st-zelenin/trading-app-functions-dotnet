using System;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace DataAccess
{
    public class TradingDbService: BaseDbService, ITradingDbService
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

            if (itemResponse == null)
            {
                throw new MissingItemResponse(azureUserId, USERS_CONTAINER_NAME);
            }

            return itemResponse.Resource;
        }
    }
}

