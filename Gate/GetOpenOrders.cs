using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Models;
using Gate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonOrder = Common.Models.Order;

namespace Gate;

internal class OrdersResponseResultItem
{
    public string currency_pair { get; set; }
    public IEnumerable<GateOrder> orders { get; set; }
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
            var response = await this.httpService.GetAsync<IEnumerable<OrdersResponseResultItem>>("/spot/open_orders");

            var body = response.Aggregate(
                new Dictionary<string, IEnumerable<CommonOrder>>(),
                (acc, raw) =>
                {
                    acc.Add(raw.currency_pair, raw.orders.Select(o => o.ToCommonOrder()));
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

