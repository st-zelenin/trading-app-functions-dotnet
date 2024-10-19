using System;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit;

public class GetHistory
{
    private readonly IByBitDbService bybitDbService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IByBitDbService bybitDbService,
        IDexDbService dexDbService,
        IDexService dexService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.bybitDbService = bybitDbService;
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

            var orders = await this.bybitDbService.GetOrdersAsync(pair, azureUserId);
            var cexOrders = orders.Where(o => o.status == ByBitOrderStatus.FILLED || o.status == ByBitOrderStatus.PARTIALLY_FILLED).Select(o => o.ToCommonOrder());

            var dexOrders = await this.dexDbService.GetOrdersAsync(pair, azureUserId, "bybit");

            var body = this.dexService.CombineCexWithDexOrders(cexOrders, dexOrders);
            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}

