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
    private readonly IBinanceDbService binanceDbService;
    private readonly IAuthService authService;

    public GetAverages(IBinanceDbService binanceDbService, IAuthService authService)
    {
        this.binanceDbService = binanceDbService;
        this.authService = authService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawAverages = await this.binanceDbService.GetAveragesAsync(azureUserId);

        var body = rawAverages.Aggregate(
            new Dictionary<string, Average>(),
            (acc, curr) =>
            {
                Average average;

                if (!acc.TryGetValue(curr.currency_pair, out average))
                {
                    average = new Average()
                    {
                        buy = new AverageSide() { money = 0, price = 0, volume = 0 },
                        sell = new AverageSide() { money = 0, price = 0, volume = 0 },
                    };

                    acc.Add(curr.currency_pair, average);
                }

                var side = curr.side == "BUY" ? average.buy : average.sell;

                side.money = curr.total_money;
                side.volume = curr.total_volume;
                side.price = curr.total_money / curr.total_volume;

                return acc;
            });


        return new OkObjectResult(body);
    }
}

