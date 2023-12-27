using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit;

internal class IHistoryRequestParams
{
    public string symbol { get; set; }
}

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

        var response = await this.httpService.GetUnsignedAsync<ResponseWithListResult_V5<Product>, SingleSpotCategoryParams>("/v5/market/instruments-info", new SingleSpotCategoryParams { });

        var body = user.bybit.Aggregate(
            new Dictionary<string, Common.Models.Product>(),
            (acc, pair) =>
            {
                var raw = response.result.list.FirstOrDefault(x => x.symbol == pair.symbol);
                if (raw != null)
                {
                    acc.Add(pair.symbol, raw.ToCommonProduct());
                }

                return acc;
            });

        return new OkObjectResult(body);
    }
}

