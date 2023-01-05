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

public class GetRecentTradeHistory
{
    private readonly IBinanceDbService binanceDbService;
    private readonly ITradingDbService tradingDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetRecentTradeHistory(
        IBinanceDbService binanceDbService,
        ITradingDbService tradingDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.binanceDbService = binanceDbService;
        this.tradingDbService = tradingDbService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("GetRecentTradeHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var side = this.httpService.GetRequiredQueryParam(req, "side");
        var limit = this.httpService.GetRequiredQueryParam(req, "limit");

        var azureUserId = this.authService.GetUserId(req);
        var user = await this.tradingDbService.GetUserAsync(azureUserId);

        var tasks = user.binance_pairs.Select(p => this.tradeHistoryService.UpdateRecentTradeHistoryAsync(p, azureUserId));
        await Task.WhenAll(tasks);

        var orders = await this.binanceDbService.GetOrdersBySide(side.ToUpper(), int.Parse(limit), azureUserId);

        var body = orders.Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}
