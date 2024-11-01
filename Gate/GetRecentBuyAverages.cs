using System;
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

namespace Gate;

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

        try
        {
            var tasks = user.gate.Select(async pair => new { pair.symbol, averages = await this.AnalyzePairAsync(pair.symbol, azureUserId) }).ToList();

            var results = await Task.WhenAll(tasks);

            var body = results.ToDictionary((result) => result.symbol, result => result.averages);

            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }

    private async Task<AverageSide> AnalyzePairAsync(string pair, string continerId)
    {
        var trades = await this.gateDbService.GetFilledOrdersAsync(pair, continerId);
        var tradesList = trades.ToList();

        var mostRecentSellIndex = tradesList.FindIndex(trade => trade.side == GateOrderSide.sell);
        var recentBuyOrders = mostRecentSellIndex == -1 ? tradesList : tradesList.GetRange(0, mostRecentSellIndex);

        return recentBuyOrders.Aggregate(
            new AverageSide() { money = 0, price = 0, volume = 0 },
            (acc, curr) =>
            {
                var totalMoney = double.Parse(curr.filled_total);

                acc.money += totalMoney;
                acc.volume += curr.type == GateOrderType.limit ? double.Parse(curr.amount) : totalMoney / double.Parse(curr.avg_deal_price);
                acc.price = acc.money / acc.volume;

                return acc;
            });
    }
}

