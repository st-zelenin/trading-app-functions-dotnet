using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using DataAccess.Interfaces;
using Binance.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using Common.Models;

namespace Binance;

public class GetHistory
{
    private readonly IBinanceDbService binanceDbService;
    private readonly IDexDbService dexDbService;
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;
    private readonly IHttpService httpService;

    public GetHistory(
        IBinanceDbService binanceDbService,
        IDexDbService dexDbService,
        IAuthService authService,
        ITradeHistoryService tradeHistoryService,
        IHttpService httpService)
    {
        this.binanceDbService = binanceDbService;
        this.dexDbService = dexDbService;
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
        this.httpService = httpService;
    }

    [FunctionName("GetHistory")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var pair = this.httpService.GetRequiredQueryParam(req, "pair");

        var azureUserId = this.authService.GetUserId(req);

        try
        {
            await this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair, azureUserId);

            var orders = await this.binanceDbService.GetOrdersAsync(pair, azureUserId);
            var cexOrders = orders.Select(o => o.ToCommonOrder());

            var dexOrders = await this.dexDbService.GetOrdersAsync(pair, azureUserId, "binance");
            var dexOrdersArr = dexOrders.ToArray();

            var body = new List<Order>();
            var dexIndex = 0;

            foreach (var cexOrder in cexOrders)
            {
                while (dexOrdersArr.Length > dexIndex && dexOrdersArr[dexIndex].updateTimestamp > cexOrder.updateTimestamp)
                {
                    body.Add(dexOrdersArr[dexIndex]);
                    dexIndex++;
                }


                body.Add(cexOrder);
            }

            if (dexIndex < dexOrdersArr.Length)
            {
                for (var i = dexIndex; i < dexOrdersArr.Length; i++)
                {
                    body.Add(dexOrdersArr[i]);
                }
            }

            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
