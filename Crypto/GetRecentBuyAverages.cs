using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using ByBitOrderSide = DataAccess.Models.CryptoOrderSide;

namespace Crypto
{
    public class GetRecentBuyAverages
    {
        private readonly ICryptoDbService cryptoDbService;
        private readonly ITradingDbService tradingDbService;
        private readonly IAuthService authService;

        public GetRecentBuyAverages(ICryptoDbService cryptoDbService, ITradingDbService tradingDbService, IAuthService authService)
        {
            this.cryptoDbService = cryptoDbService;
            this.tradingDbService = tradingDbService;
            this.authService = authService;
        }

        [FunctionName("GetRecentBuyAverages")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUserId = this.authService.GetUserId(req);
            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            var body = new Dictionary<string, AverageSide>();
            foreach (var pair in user.crypto_pairs)
            {
                body.Add(pair, await this.AnalyzePairAsync(pair, azureUserId));
            }

            return new OkObjectResult(body);
        }

        private async Task<AverageSide> AnalyzePairAsync(string pair, string continerId)
        {
            var trades = await this.cryptoDbService.GetFilledOrdersAsync(pair, continerId);

            var lastSell = trades.FirstOrDefault(trade => trade.side == ByBitOrderSide.SELL);

            var recent = lastSell == null ? trades
                : trades.Where(trade => trade.side == ByBitOrderSide.BUY && trade.update_time > lastSell.create_time);

            return recent.Aggregate(
                new AverageSide() { money = 0, price = 0, volume = 0 },
                (acc, curr) =>
                {
                    acc.money += curr.cumulative_value;
                    acc.volume += curr.cumulative_quantity;
                    acc.price = acc.money / acc.volume;

                    return acc;
                });
        }
    }
}

