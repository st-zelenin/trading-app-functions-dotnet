﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit;

public class GetAverages
{
    private readonly IByBitDbService bybitDbService;
    private readonly IAuthService authService;
    private readonly IDexDbService dexDbService;

    public GetAverages(IByBitDbService bybitDbService, IAuthService authService, IDexDbService dexDbService)
    {
        this.bybitDbService = bybitDbService;
        this.authService = authService;
        this.dexDbService = dexDbService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.bybitDbService.GetAveragesAsync(azureUserId);
        var rawDexAverages = await this.dexDbService.GetAveragesAsync(azureUserId, "binance");

        var body = rawCexAverages.Concat(rawDexAverages).Aggregate(
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

                var side = curr.side.ToUpper() == "BUY" ? average.buy : average.sell;

                side.money = curr.total_money;
                side.volume = curr.total_volume;
                side.price = curr.total_money / curr.total_volume;

                return acc;
            });


        return new OkObjectResult(body);
    }
}

