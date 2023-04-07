using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using DataAccess.Interfaces;
using Common.Models;

namespace User;

public class OrderPairs
{
    private readonly IBaseHttpService baseHttpService;
    private readonly IAuthService authService;
    private readonly ITradingDbService tradingDbService;

    public OrderPairs(IBaseHttpService baseHttpService, IAuthService authService, ITradingDbService tradingDbService)
    {
        this.baseHttpService = baseHttpService;
        this.authService = authService;
        this.tradingDbService = tradingDbService;
    }

    [FunctionName("OrderPairs")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        var orderedSymbols = await this.baseHttpService.GetRequestBody<OrderedSymbols>(req);
        if (orderedSymbols == null)
        {
            throw new ArgumentException("\"orderedSymbols\" is missing");
        }

        var azureUserId = this.authService.GetUserId(req);

        var body = await this.tradingDbService.OrderPairsAsync(azureUserId, orderedSymbols);

        return new OkObjectResult(body);
    }
}

