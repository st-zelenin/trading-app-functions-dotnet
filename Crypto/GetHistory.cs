using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Primitives;

namespace Crypto
{
    public class GetHistory
    {
        private readonly ICryptoDbService cryptoDbService;
        private readonly IAuthService authService;
        private readonly ITradeHistoryService tradeHistoryService;
        private readonly IHttpService httpService;

        public GetHistory(
            ICryptoDbService cryptoDbService,
            IAuthService authService,
            ITradeHistoryService tradeHistoryService,
            IHttpService httpService)
        {
            this.cryptoDbService = cryptoDbService;
            this.authService = authService;
            this.tradeHistoryService = tradeHistoryService;
            this.httpService = httpService;
        }

        [FunctionName("GetHistory")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var pair = this.httpService.GetRequiredQueryParam(req, "pair");

            var azureUserId = this.authService.GetUserId(req.Headers["Authorization"]);

            await this.tradeHistoryService.UpdateRecentTradeHistory(azureUserId);

            var orders = await this.cryptoDbService.GetOrdersAsync(pair, azureUserId);

            var body = orders.Select(o => o.ToCommonOrder());

            return new OkObjectResult(body);
        }
    }

    internal class ImportPeriodTradeHistoryRequestData
    {
        public long start_ts { get; set; }
        public long end_ts { get; set; }
    }
}

