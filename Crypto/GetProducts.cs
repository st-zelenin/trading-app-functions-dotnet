using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto
{
    public class GetProducts
    {
        private readonly ITradingDbService tradingDbService;
        private readonly IHttpService httpService;
        private readonly IAuthService authService;

        public GetProducts(ITradingDbService tradingDbService, IHttpService httpService, IAuthService authService)
        {
            this.tradingDbService = tradingDbService;
            this.httpService = httpService;
            this.authService = authService;
        }

        [FunctionName("GetProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req
        )
        {
            var azureUserId = this.authService.GetUserId(req.Headers["Authorization"]);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var instruments = await this.httpService.GetAsync<ResponseWithResult<InstrumentsResponseResult>>("public/get-instruments");

            var body = user.crypto_pairs.Aggregate(
                new Dictionary<string, Common.Models.Product>(),
                (acc, pair) =>
                {
                    var raw = instruments.result.instruments.First(x => x.instrument_name == pair);
                    if (raw != null)
                    {
                        acc.Add(pair, raw.ToCommonProduct());
                    }

                    return acc;
                });

            return new OkObjectResult(body);
        }
    }

}

