using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using Gate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;


public class GetRecentTradeHistory
{
    private readonly IGateDbService gateDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;
    private readonly ITradingDbService tradingDbService;

    public GetRecentTradeHistory(
        IGateDbService gateDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService,
        ITradingDbService tradingDbService)
    {
        this.gateDbService = gateDbService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
        this.tradingDbService = tradingDbService;
    }

    [FunctionName("GetRecentTradeHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var side = this.httpService.GetRequiredQueryParam(req, "side");
        var limit = this.httpService.GetRequiredQueryParam(req, "limit");

        var azureUserId = this.authService.GetUserId(req);
        var user = await this.tradingDbService.GetUserAsync(azureUserId);

        //foreach(var pair in user.pairs)
        //{
        //    await this.tradeHistoryService.ImportTradeHistoryAsync(pair, azureUserId);
        //}

        await Task.WhenAll(user.gate.Select(p => this.tradeHistoryService.UpdateRecentTradeHistory(p.symbol, azureUserId)));

        var orders = await this.gateDbService.GetOrdersBySide(side.ToUpper(), int.Parse(limit), azureUserId);

        var body = orders.Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}

