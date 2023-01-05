using System;
using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace User
{
    public class UpdateUser
    {
        private readonly IBaseHttpService baseHttpService;
        private readonly IAuthService authService;
        private readonly ITradingDbService tradingDbService;

        public UpdateUser(IBaseHttpService baseHttpService, IAuthService authService, ITradingDbService tradingDbService)
        {
            this.baseHttpService = baseHttpService;
            this.authService = authService;
            this.tradingDbService = tradingDbService;
        }

        [FunctionName("UpdateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var updatedUser = await this.baseHttpService.GetRequestBody<Trader>(req);
            if (updatedUser == null)
            {
                throw new ArgumentException("\"user\" is missing");
            }

            var azureUserId = this.authService.GetUserId(req);

            var user = await this.tradingDbService.GetUserAsync(azureUserId);

            user.pairs = updatedUser.pairs;
            user.crypto_pairs = updatedUser.crypto_pairs;
            user.coinbase_pairs = updatedUser.coinbase_pairs;
            user.bybit_pairs = updatedUser.bybit_pairs;
            user.binance_pairs = updatedUser.binance_pairs;

            var body = await this.tradingDbService.UpdateUserAsync(user);

            return new OkObjectResult(body);
        }
    }
}

