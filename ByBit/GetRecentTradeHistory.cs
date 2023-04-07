using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit;

public class GetRecentTradeHistory
{
    private readonly IByBitDbService bybitDbService;
    private readonly ITradingDbService tradingDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetRecentTradeHistory(
        IByBitDbService bybitDbService,
        ITradingDbService tradingDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.bybitDbService = bybitDbService;
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

        var tasks = user.bybit.Select(p => this.tradeHistoryService.UpdateRecentTradeHistoryAsync(p.symbol, azureUserId));
        await Task.WhenAll(tasks);

        var orders = await this.bybitDbService.GetOrdersBySide(side.ToUpper(), int.Parse(limit), azureUserId);

        var body = orders.Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}
