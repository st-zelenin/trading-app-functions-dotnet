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
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IGateDbService cryptoDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.gateDbService = cryptoDbService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("GetHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");

        var azureUserId = this.authService.GetUserId(req);

        await this.tradeHistoryService.UpdateRecentTradeHistory(pair, azureUserId);

        var orders = await this.gateDbService.GetOrdersAsync(pair, azureUserId);

        var body = orders.Where(o => o.status == GateOrderStatus.closed).Select(o => o.ToCommonOrder());

        return new OkObjectResult(body);
    }
}

