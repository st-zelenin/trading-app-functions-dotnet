using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using Common;
using Microsoft.Azure.Cosmos;
using Crypto.Models;
using System.Linq;
using System.Collections.Generic;
using Common.Interfaces;

namespace Crypto
{
    public class GetProducts
    {
        //static readonly HttpClient client = new HttpClient();

        private readonly IDbService dbService;

        public GetProducts(IDbService dbService)
        {
            this.dbService = dbService;
        }

        [FunctionName("GetProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req
            )
        {
            var azureUserId = AuthService.GetUserId(req.Headers["Authorization"]);

            //var dbService = new DbService(this.secretsService);
            var usersContainer = this.dbService.GetUsersContainer();

            var itemResponse = await usersContainer.ReadItemAsync<Common.Models.User>(azureUserId, new PartitionKey(azureUserId));
            var user = itemResponse.Resource;


            HttpResponseMessage response = await new HttpClient().GetAsync("https://api.crypto.com/v2/public/get-instruments");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var instruments = JsonConvert.DeserializeObject<InstrumentsResponse>(responseBody);

            var products = user.crypto_pairs.Aggregate(
                new Dictionary<string, Common.Models.Product>(),
                (acc, pair) =>
                {
                    var raw = instruments.result.instruments.First(x => x.instrument_name == pair);

                    if (raw != null)
                    {
                        acc.Add(pair, new Common.Models.Product()
                        {
                            currencyPair = raw.instrument_name,
                            minQuantity = decimal.Parse(raw.min_quantity),
                            minTotal = 0,
                            pricePrecision = 1 / Math.Pow(10, raw.price_decimals),
                        });
                    }

                    return acc;
                });

            return new OkObjectResult(products);
        }
    }
}

