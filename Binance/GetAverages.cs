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
using Common.Models;
using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Binance;

public class GetAverages
{
    private readonly IAuthService authService;
    private readonly IBinanceDbService binanceDbService;
    private readonly IDexDbService dexDbService;

    public GetAverages(IAuthService authService, IBinanceDbService binanceDbService, IDexDbService dexDbService)
    {
        this.authService = authService;
        this.binanceDbService = binanceDbService;
        this.dexDbService = dexDbService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.binanceDbService.GetAveragesAsync(azureUserId);
        var rawDexverages = await this.dexDbService.GetAveragesAsync(azureUserId, "binance");

        var body = rawCexAverages.Concat(rawDexverages).Aggregate(
            new Dictionary<string, Average>(),
            (acc, curr) =>
            {
                if (!acc.TryGetValue(curr.currency_pair, out Average average))
                {
                    average = new Average()
                    {
                        buy = new AverageSide() { money = 0, price = 0, volume = 0 },
                        sell = new AverageSide() { money = 0, price = 0, volume = 0 },
                    };

                    acc.Add(curr.currency_pair, average);
                }

                var side = curr.side.ToLower() == "buy" ? average.buy : average.sell;

                side.money += curr.total_money;
                side.volume += curr.total_volume;
                side.price = side.money / side.volume;

                return acc;
            });


        return new OkObjectResult(body);
    }
}

