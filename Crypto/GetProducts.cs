using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto
{
    internal class InstrumentsResponseResult
    {
        public IEnumerable<Instrument> instruments { get; set; }
    }

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
            var azureUserId = this.authService.GetUserId(req);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var instruments = await this.httpService.GetAsync<ResponseWithResult<InstrumentsResponseResult>>("public/get-instruments");

            var body = user.crypto_pairs.Aggregate(
                new Dictionary<string, Common.Models.Product>(),
                (acc, pair) =>
                {
                    // for some reason crypto.com does not return products for '_USDC'
                    // replacing with '_USDT'
                    var usdtPair = pair.Replace("_USDC", "_USDT");
                    var raw = instruments.result.instruments.First(x => x.instrument_name == usdtPair);
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
