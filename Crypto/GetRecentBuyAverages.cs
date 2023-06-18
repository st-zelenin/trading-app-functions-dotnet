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

namespace Crypto;

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

        var tasks = user.crypto.Select(async pair => new { pair.symbol, averages = await this.AnalyzePairAsync(pair.symbol, azureUserId) }).ToList();

        var results = await Task.WhenAll(tasks);

        var body = results.ToDictionary(result => result.symbol, result => result.averages);

        return new OkObjectResult(body);
    }

    private async Task<AverageSide> AnalyzePairAsync(string pair, string continerId)
    {
        var trades = await this.cryptoDbService.GetFilledOrdersAsync(pair, continerId);
        var tradesList = trades.ToList();

        var mostRecentSellIndex = tradesList.FindIndex(trade => trade.side == CryptoOrderSide.SELL);
        var recentBuyOrders = mostRecentSellIndex == -1 ? tradesList : tradesList.GetRange(0, mostRecentSellIndex);

        return recentBuyOrders.Aggregate(
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
