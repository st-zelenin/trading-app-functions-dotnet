using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonBalance = Common.Models.Balance;

namespace Crypto;

internal class BalancesResponseResult
{
    public IEnumerable<Balance> data { get; set; }
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

        var response = await this.httpService.PostAsync<ResponseWithResult<BalancesResponseResult>>("private/user-balance");

        var body = response.result.data.First().position_balances.Aggregate(
            new Dictionary<string, CommonBalance>(),
            (acc, raw) =>
            {
                acc.Add(raw.instrument_name, raw.ToCommonBalance());
                return acc;
            });

        return new OkObjectResult(body);
    }
}

