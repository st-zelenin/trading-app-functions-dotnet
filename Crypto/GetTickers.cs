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
    public class TickersResponseResult
    {
        public IEnumerable<Ticker> data { get; set; }
    }

    public class GetTickers
    {
        private readonly ITradingDbService tradingDbService;
        private readonly IHttpService httpService;
        private readonly IAuthService authService;

        public GetTickers(ITradingDbService tradingDbService, IHttpService httpService, IAuthService authService)
        {
            this.tradingDbService = tradingDbService;
            this.httpService = httpService;
            this.authService = authService;
        }

        [FunctionName("GetTickers")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUserId = this.authService.GetUserId(req.Headers["Authorization"]);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var response = await this.httpService.GetAsync<ResponseWithResult<TickersResponseResult>>("public/get-ticker");

            var body = user.crypto_pairs.Aggregate(
                new Dictionary<string, Common.Models.Ticker>(),
                (acc, pair) =>
                {
                    var raw = response.result.data.First(x => x.i == pair);
                    if (raw != null)
                    {
                        acc.Add(pair, raw.ToCommonTicker());
                    }

                    return acc;
                });

            return new OkObjectResult(body);
        }
    }
}
