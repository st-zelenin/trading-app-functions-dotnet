using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Interfaces;
using DataAccess.Interfaces;
using Binance.Interfaces;
using System.Linq;

namespace Binance;

public class GetHistory
{
    private readonly IBinanceDbService binanceDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IBinanceDbService binanceDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.binanceDbService = binanceDbService;
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

        await this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair, azureUserId);

        var orders = await this.binanceDbService.GetOrdersAsync(pair, azureUserId);

        var body = orders.Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}
