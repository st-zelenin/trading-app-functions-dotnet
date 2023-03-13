using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit
{
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
            var azureUserId = this.authService.GetUserId(req);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var response = await this.httpService.GetAsync<ResponseWithResult<IEnumerable<Ticker>>>("/spot/quote/v1/ticker/24hr");

            var body = user.bybit_pairs.Aggregate(
                new Dictionary<string, Common.Models.Ticker>(),
                (acc, pair) =>
                {
                    var raw = response.result.FirstOrDefault(x => x.symbol == pair);
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
