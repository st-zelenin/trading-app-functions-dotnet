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
using System.Collections.Generic;
using Binance.Models;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Binance;

public class GetTickers
{
    private readonly ITradingDbService tradingDbService;
    private readonly IHttpService httpService;
    private readonly IAuthService authService;

    public GetTickers(ITradingDbService tradingDbService, IHttpService httpService, IAuthService authService)
    {
        this.tradingDbService = tradingDbService;
        this.httpService = httpService;
        this.authService = authService;
    }

    [FunctionName("GetTickers")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
    {
        var azureUserId = this.authService.GetUserId(req);
        var user = await this.tradingDbService.GetUserAsync(azureUserId);

        var tickers = await this.httpService.GetAsync<IEnumerable<Ticker>, GetTickersParams>("/api/v3/ticker/24hr", new GetTickersParams { symbols = user.binance_pairs });

        var body = tickers.Aggregate(
            new Dictionary<string, Common.Models.Ticker>(),
            (acc, pair) =>
            {
                acc.Add(pair.symbol, pair.ToCommonTicker());

                return acc;
            });

        return new OkObjectResult(body);
    }
}

class AllOrdersParams
{
    public string symbol { get; set; }
}

class GetTickersParams
{
    public IEnumerable<string> symbols { get; set; }
}
