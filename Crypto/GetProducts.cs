using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Crypto
{
    public class GetProducts
    {
        private readonly IDbService dbService;
        private readonly IHttpService httpService;
        private readonly IAuthService authService;

        public GetProducts(IDbService dbService, IHttpService httpService, IAuthService authService)
        {
            this.dbService = dbService;
            this.httpService = httpService;
            this.authService = authService;
        }

        [FunctionName("GetProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req
        )
        {
            var azureUserId = this.authService.GetUserId(req.Headers["Authorization"]);

            var usersContainer = await this.dbService.GetUsersContainer();

            var itemResponse = await usersContainer.ReadItemAsync<Common.Models.User>(azureUserId, new PartitionKey(azureUserId));
            var user = itemResponse.Resource;

            HttpResponseMessage response = await new HttpClient().GetAsync("https://api.crypto.com/v2/public/get-instruments");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var instruments = JsonConvert.DeserializeObject<ResponseWithResult<InstrumentsResponseResult>>(responseBody);

            var body = user.crypto_pairs.Aggregate(
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

            return new OkObjectResult(body);
        }
    }

}

