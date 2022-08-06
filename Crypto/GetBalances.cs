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

namespace Crypto
{
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
            this.authService.ValidateUser(req.Headers["Authorization"]);

            var response = await this.httpService.PostAsync<ResponseWithResult<BalancesResponseResult>>("private/get-account-summary");

            var body = response.result.accounts.Aggregate(
                new Dictionary<string, CommonBalance>(),
                (acc, raw) =>
                {
                    acc.Add(raw.currency, raw.ToCommonBalance());
                    return acc;
                });

            return new OkObjectResult(body);
        }
    }
}

