using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using Gate.Interfaces;
using Gate.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;

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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);
        var user = await this.tradingDbService.GetUserAsync(azureUserId);

        try
        {
            var tickers = await this.httpService.GetAsync<IEnumerable<Ticker>>("/spot/tickers");

            var body = user.gate.Aggregate(
                new Dictionary<string, Common.Models.Ticker>(),
                (acc, pair) =>
                {
                    var raw = tickers.FirstOrDefault(x => x.currency_pair == pair.symbol);
                    if (raw != null)
                    {
                        acc.Add(pair.symbol, raw.ToCommonTicker());
                    }

                    return acc;
                });

            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}

