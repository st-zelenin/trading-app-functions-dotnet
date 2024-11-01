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
using Binance.Interfaces;
using DataAccess.Models;
using System.Collections.Generic;
using CommonOrder = Common.Models.Order;
using System.Linq;

namespace Binance;

public class GetOpenOrders
{
    private readonly IAuthService authService;
    private readonly IHttpService httpService;

    public GetOpenOrders(IAuthService authService, IHttpService httpService)
    {
        this.authService = authService;
        this.httpService = httpService;
    }

    [FunctionName("GetOpenOrders")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        this.authService.ValidateUser(req);

        try
        {
            var response = await this.httpService.GetSignedAsync<IEnumerable<BinanceOrder>>("/api/v3/openOrders");

            var body = response.Aggregate(
                new Dictionary<string, IList<CommonOrder>>(),
                (acc, raw) =>
                {
                    if (!acc.TryGetValue(raw.symbol, out IList<CommonOrder> instrument))
                    {
                        instrument = new List<CommonOrder>();
                        acc.Add(raw.symbol, instrument);
                    }

                    instrument.Add(raw.ToCommonOrder());

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
