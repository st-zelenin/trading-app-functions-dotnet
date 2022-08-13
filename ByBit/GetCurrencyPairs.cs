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

namespace ByBit
{
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

            var response = await this.httpService.GetAsync<ResponseWithResult<IEnumerable<Product>>>("/spot/v1/symbols");

            var body = response.result.Select(t => t.name);

            return new OkObjectResult(body);
        }
    }
}
