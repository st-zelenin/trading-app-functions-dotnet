using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonOrder = Common.Models.Order;

namespace ByBit;

internal class GetOpenOrdersRequestParams
{
    public string category { get; set; }
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

        var response = await this.httpService.GetV5Async<ResponseWithListResult_V5<OrderV5>, GetOpenOrdersRequestParams>("/v5/order/realtime", new GetOpenOrdersRequestParams() { category = "spot" });

        var body = response.result.list.Aggregate(
            new Dictionary<string, IList<CommonOrder>>(),
            (acc, raw) =>
            {
                IList<CommonOrder> instrument;

                if (!acc.TryGetValue(raw.symbol, out instrument))
                {
                    instrument = new List<CommonOrder>();
                    acc.Add(raw.symbol, instrument);
                }

                instrument.Add(raw.ToByBitOrder().ToCommonOrder());

                return acc;
            });


        return new OkObjectResult(body);
    }
}
