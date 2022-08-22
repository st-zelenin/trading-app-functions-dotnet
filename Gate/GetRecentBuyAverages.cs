using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate
{
    public class GetRecentBuyAverages
    {
        private readonly IGateDbService gateDbService;
        private readonly ITradingDbService tradingDbService;
        private readonly IAuthService authService;

        public GetRecentBuyAverages(IGateDbService cryptoDbService, ITradingDbService tradingDbService, IAuthService authService)
        {
            this.gateDbService = cryptoDbService;
            this.tradingDbService = tradingDbService;
            this.authService = authService;
        }

        [FunctionName("GetRecentBuyAverages")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUserId = this.authService.GetUserId(req);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var body = new Dictionary<string, AverageSide>();
            foreach (var pair in user.pairs)
            {
                body.Add(pair, await this.AnalyzePairAsync(pair, azureUserId));
            }

            return new OkObjectResult(body);
        }

        private async Task<AverageSide> AnalyzePairAsync(string pair, string continerId)
        {
            var trades = await this.gateDbService.GetFilledOrdersAsync(pair, continerId);

            var lastSell = trades.FirstOrDefault(trade => trade.side == GateOrderSide.sell);

            var recent = lastSell == null ? trades
                : trades.Where(trade => trade.side == GateOrderSide.buy && trade.update_time_ms > lastSell.create_time_ms);

            return recent.Aggregate(
                new AverageSide() { money = 0, price = 0, volume = 0 },
                (acc, curr) =>
                {
                    acc.money += double.Parse(curr.filled_total);
                    acc.volume += double.Parse(curr.amount);
                    acc.price = acc.money / acc.volume;

                    return acc;
                });
        }
    }
}

