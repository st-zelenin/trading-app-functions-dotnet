using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using DataAccess.Interfaces;
using Binance.Interfaces;
using System.Linq;
using System;

namespace Binance;

public class GetHistory
{
    private readonly IBinanceDbService binanceDbService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IBinanceDbService binanceDbService,
        IDexDbService dexDbService,
        IDexService dexService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.binanceDbService = binanceDbService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("GetHistory")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");

        var azureUserId = this.authService.GetUserId(req);

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair, azureUserId);

            var orders = await this.binanceDbService.GetOrdersAsync(pair, azureUserId);
            var cexOrders = orders.Select(o => o.ToCommonOrder());

            var dexOrders = await this.dexDbService.GetOrdersAsync(pair, azureUserId, "binance");

            var body = this.dexService.CombineCexWithDexOrders(cexOrders, dexOrders);
            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
