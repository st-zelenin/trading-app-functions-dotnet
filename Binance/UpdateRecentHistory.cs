using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using Binance.Interfaces;
using System;

namespace Binance;

public class UpdateRecentHistory
{
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public UpdateRecentHistory(
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("UpdateRecentHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");
        var azureUserId = this.authService.GetUserId(req);

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair, azureUserId);

            return new OkResult();
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
