using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonBalance = Common.Models.Balance;

namespace ByBit;

internal class BalancesResponseResult
{
    public IEnumerable<Balance> balances { get; set; }
}

public class GetBalances
{
    private readonly IAuthService authService;
    private readonly IHttpService httpService;

    public GetBalances(IAuthService authService, IHttpService httpService)
    {
        this.authService = authService;
        this.httpService = httpService;
    }

    [FunctionName("GetBalances")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        this.authService.ValidateUser(req);

        var response = await this.httpService.GetV5Async<ResponseWithResult<BalancesResponseResult>>("/spot/v3/private/account");

        var body = response.result.balances.Aggregate(
            new Dictionary<string, CommonBalance>(),
            (acc, raw) =>
            {
                acc.Add(raw.coinId, new CommonBalance() { available = double.Parse(raw.free), locked = double.Parse(raw.locked) });
                return acc;
            });

        return new OkObjectResult(body);
    }
}

