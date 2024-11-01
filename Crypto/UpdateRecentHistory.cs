using System;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto;

public class UpdateRecentHistory
{
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;

    public UpdateRecentHistory(
        IAuthService authService,
        ITradeHistoryService tradeHistoryService)
    {
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
    }

    [FunctionName("UpdateRecentHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistory(azureUserId);
            return new OkResult();
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}

