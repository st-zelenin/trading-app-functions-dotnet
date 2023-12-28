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
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IByBitDbService bybitDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.bybitDbService = bybitDbService;
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

        var orders = await this.bybitDbService.GetOrdersAsync(pair, azureUserId);

        var body = orders.Where(o => o.status == ByBitOrderStatus.FILLED || o.status == ByBitOrderStatus.PARTIALLY_FILLED).Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}

