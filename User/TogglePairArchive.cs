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

public class TogglePairArchive
{
    private readonly IBaseHttpService baseHttpService;
    private readonly IAuthService authService;
    private readonly ITradingDbService tradingDbService;

    public TogglePairArchive(IBaseHttpService baseHttpService, IAuthService authService, ITradingDbService tradingDbService)
    {
        this.baseHttpService = baseHttpService;
        this.authService = authService;
        this.tradingDbService = tradingDbService;
    }

    [FunctionName("TogglePairArchive")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        var exchangeSymbol = await this.baseHttpService.GetRequestBody<ExchangeSymbol>(req);
        if (exchangeSymbol == null)
        {
            throw new ArgumentException("\"exchangeSymbol\" is missing");
        }

        var azureUserId = this.authService.GetUserId(req);

        var body = await this.tradingDbService.TogglePairArchiveAsync(azureUserId, exchangeSymbol);

        return new OkObjectResult(body);
    }
}

