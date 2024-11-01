using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonOrder = Common.Models.Order;

namespace Crypto;


internal class OrdersResponseResult
{
    public IEnumerable<CryptoOrder> data { get; set; }
}

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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        this.authService.ValidateUser(req);

        try
        {
            var response = await this.httpService.PostAsync<ResponseWithResult<OrdersResponseResult>>("private/get-open-orders");

            var body = response.result.data.Aggregate(
                new Dictionary<string, List<CommonOrder>>(),
                (acc, raw) =>
                {
                    List<CommonOrder> instrument;

                    if (!acc.TryGetValue(raw.instrument_name, out instrument))
                    {
                        instrument = new();
                        acc.Add(raw.instrument_name, instrument);
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

