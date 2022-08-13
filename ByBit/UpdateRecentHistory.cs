using System.Threading.Tasks;
using ByBit.Interfaces;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit
{
    public class UpdateRecentHistory
    {
        private readonly IAuthService authService;
        private readonly ITradeHistoryService tradeHistoryService;
        private readonly IHttpService httpService;

        public UpdateRecentHistory(
            IAuthService authService,
            ITradeHistoryService tradeHistoryService,
            IHttpService httpService)
        {
            this.authService = authService;
            this.tradeHistoryService = tradeHistoryService;
            this.httpService = httpService;
        }

        [FunctionName("UpdateRecentHistory")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            var pair = this.httpService.GetRequiredQueryParam(req, "pair");

            var azureUserId = this.authService.GetUserId(req);

            await this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair, azureUserId);

            return new OkResult();
        }
    }
}
