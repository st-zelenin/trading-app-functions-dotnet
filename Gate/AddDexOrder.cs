using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using Gate.Interfaces;

namespace Gate;

public class AddDexOrder
{
    private readonly IHttpService httpService;
    private readonly IAuthService authService;
    private readonly IDexDbService dexDbService;

    public AddDexOrder(IHttpService httpService, IAuthService authService, IDexDbService dexDbService)
    {
        this.httpService = httpService;
        this.authService = authService;
        this.dexDbService = dexDbService;
    }

    [FunctionName("AddDexOrder")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var order = await this.httpService.GetRequestBody<NewOrder>(req) ?? throw new ArgumentException("\"order\" is missing");
        var azureUserId = this.authService.GetUserId(req);

        var dexOrder = NewOrder.ToNewDexOrder(order, "gate");

        try
        {
            var result = await this.dexDbService.UpsertOrderAsync(dexOrder, azureUserId);
            return new OkObjectResult(result);

        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}

