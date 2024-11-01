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
using CommonBalance = Common.Models.Balance;

namespace Gate
{
    internal class BalancesResponseResult
    {
        public IEnumerable<Balance> accounts { get; set; }
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

            try
            {
                var response = await this.httpService.GetAsync<IEnumerable<Balance>>("/spot/accounts");

                var body = response.Aggregate(
                    new Dictionary<string, CommonBalance>(),
                    (acc, raw) =>
                    {
                        acc.Add(raw.currency, raw.ToCommonBalance());
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
}
