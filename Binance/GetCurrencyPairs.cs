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
using System.Collections.Generic;
using Binance.Interfaces;
using Binance.Models;
using System.Linq;

namespace Binance;

public class GetCurrencyPairs
{
    private readonly IHttpService httpService;

    public GetCurrencyPairs(IAuthService authService, IHttpService httpService)
    {
        this.httpService = httpService;
    }

    [FunctionName("GetCurrencyPairs")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var exchangeInfo = await this.httpService.GetAsync<ExchangeInfo<BaseProduct>>("/api/v3/exchangeInfo?permissions=SPOT");

        var body = exchangeInfo.symbols.Where(s => s.quoteAsset == "USDT" && s.status == "TRADING").Select(s => s.symbol);

        return new OkObjectResult(body);
    }
}

