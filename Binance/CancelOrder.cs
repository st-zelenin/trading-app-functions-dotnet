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

namespace Binance;

internal class CancelOrderRequestData
{
    public string id { get; set; }
    public string pair { get; set; }
}

internal class CancelOrderRequestParams
{
    public string symbol { get; set; }
    public long orderId { get; set; }
}

public class CancelOrder
{
    private readonly IHttpService httpService;
    private readonly IAuthService authService;

    public CancelOrder(IHttpService httpService, IAuthService authService)
    {
        this.httpService = httpService;
        this.authService = authService;
    }

    [FunctionName("CancelOrder")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        var data = await this.httpService.GetRequestBody<CancelOrderRequestData>(req);

        if (data == null || data.id == null)
        {
            throw new ArgumentNullException("id is missing");
        }

        this.authService.ValidateUser(req);

        var body = await this.httpService.DeleteSignedAsync<BinanceOrder, CancelOrderRequestParams>("/spot/v1/order",
            new CancelOrderRequestParams() { orderId = long.Parse(data.id), symbol = data.pair });

        return new OkObjectResult(body);
    }
}

