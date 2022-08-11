using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto
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

            var response = await this.httpService.GetAsync<ResponseWithResult<TickersResponseResult>>("public/get-ticker");

            var body = response.result.data.Select(t => t.i);

            return new OkObjectResult(body);
        }
    }
}

