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
using CommonBalance = Common.Models.Balance;
using Binance.Models;
using System.Linq;

namespace Binance;

public class GetBalances
{
    private readonly IAuthService authService;
    private readonly IHttpService httpService;

    public GetBalances(IAuthService authService, IHttpService httpService)
    {
        this.authService = authService;
        this.httpService = httpService;
    }

    [FunctionName("GetBalances")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        this.authService.ValidateUser(req);

        try
        {
            var response = await this.httpService.PostSignedAsync<IEnumerable<Balance>>("/sapi/v3/asset/getUserAsset");

            var body = response.Aggregate(
                new Dictionary<string, CommonBalance>(),
                (acc, raw) =>
                {
                    acc.Add(raw.asset, new CommonBalance() { available = double.Parse(raw.free), locked = double.Parse(raw.locked) });
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

