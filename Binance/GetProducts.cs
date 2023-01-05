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
using System.Collections.Generic;
using Binance.Interfaces;
using Binance.Models;
using System.Linq;

namespace Binance;

public class GetProducts
{
    private readonly ITradingDbService tradingDbService;
    private readonly IHttpService httpService;
    private readonly IAuthService authService;

    public GetProducts(ITradingDbService tradingDbService, IHttpService httpService, IAuthService authService)
    {
        this.tradingDbService = tradingDbService;
        this.httpService = httpService;
        this.authService = authService;
    }

    [FunctionName("GetProducts")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);
        var user = await this.tradingDbService.GetUserAsync(azureUserId);

        var exchangeInfo = await this.httpService.GetAsync<ExchangeInfo<Product>, GetProductsParams>("/api/v3/exchangeInfo", new GetProductsParams { symbols = user.binance_pairs });

        var body = exchangeInfo.symbols.Aggregate(
            new Dictionary<string, Common.Models.Product>(),
            (acc, product) =>
            {
                acc.Add(product.symbol, product.ToCommonProduct());

                return acc;
            });

        return new OkObjectResult(body);
    }
}

internal class GetProductsParams
{
    public IEnumerable<string> symbols { get; set; }
}
