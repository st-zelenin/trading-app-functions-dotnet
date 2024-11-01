using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Gate.Interfaces;
using Gate.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;

public class GetCurrencyPairs
{
    private readonly IAuthService authService;
    private readonly IHttpService httpService;

    public GetCurrencyPairs(IAuthService authService, IHttpService httpService)
    {
        this.authService = authService;
        this.httpService = httpService;
    }

    [FunctionName("GetCurrencyPairs")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        this.authService.ValidateUser(req);

        try
        {
            var products = await this.httpService.GetAsync<IEnumerable<Product>>("/spot/currency_pairs");
            var body = products.Select(p => p.id);

            return new OkObjectResult(body);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}

