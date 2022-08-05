using System;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Common
{


    public class DbService
    {
        //[Newtonsoft.Json.JsonConverter(typeof(Microsoft.Azure.Cosmos.CosmosClientOptions+ ClientOptionJsonConverter))]
        //public Microsoft.Azure.Cosmos.CosmosSerializer Serializer { get; set; }

        public DbService()
        {
        }

        private CosmosClient GetClient()
        {
            string endpoint = EnvironmentVariableService.GetVariable(EnvironmentVariableService.Keys.CosmosDbEndpoint);
            string key = SecretsService.GetSecret(SecretsService.Keys.CosmosClient);

            ////CosmosSerializer ignoreNullSerializer = new MyCustomIgnoreNullSerializer();

            //CosmosClientOptions clientOptions = new CosmosClientOptions()
            //{
            //    Serializer = new Newtonsoft.Json.JsonSerializer()
            //};

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

