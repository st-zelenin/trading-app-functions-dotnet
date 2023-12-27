using System;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace ByBit;

internal class CancelOrderRequestData
{
    public string id { get; set; }
    public string pair { get; set; }
}

internal class CancelOrderRequestParams
{
    public string category = "spot";
    public string orderId { get; set; }
    public string symbol { get; set; }
}

internal class CancelOrderResponce
{
    public string orderId { get; set; }
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

        if (data == null || data.id == null || data.pair == null)
        {
            throw new ArgumentNullException("id or pair is missing");
        }

        this.authService.ValidateUser(req);

        var requestBody = new CancelOrderRequestParams() { orderId = data.id, symbol = data.pair };

        var body = await this.httpService.PostAsync<ResponseWithResult<CancelOrderResponce>>("/v5/order/cancel", JsonConvert.SerializeObject(requestBody));

        return new OkObjectResult(body);
    }
}
