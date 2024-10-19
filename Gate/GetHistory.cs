using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Gate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;

public class GetHistory
{
    private readonly IGateDbService gateDbService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IGateDbService cryptoDbService,
        IDexDbService dexDbService,
        IDexService dexService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.gateDbService = cryptoDbService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("GetHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");

        var azureUserId = this.authService.GetUserId(req);

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistory(pair, azureUserId);

            var orders = await this.gateDbService.GetOrdersAsync(pair, azureUserId);
            var cexOrders = orders.Where(o => o.status == GateOrderStatus.closed).Select(o => o.ToCommonOrder());

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

