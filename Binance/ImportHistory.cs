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
using Binance.Interfaces;

namespace Binance;

public class ImportHistory
{
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public ImportHistory(
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("ImportHistory")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");

        var azureUserId = this.authService.GetUserId(req);

        await this.tradeHistoryService.ImportTradeHistoryAsync(pair, azureUserId);

        return new OkResult();
    }
}

