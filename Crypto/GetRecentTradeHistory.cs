using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto;

public class GetRecentTradeHistory
{
    private readonly ICryptoDbService cryptoDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetRecentTradeHistory(
        ICryptoDbService cryptoDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.cryptoDbService = cryptoDbService;
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

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistory(azureUserId);

            var orders = await this.cryptoDbService.GetOrdersBySide(side.ToUpper(), int.Parse(limit), azureUserId);

            var body = orders.Select(o => o.ToCommonOrder());

            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
