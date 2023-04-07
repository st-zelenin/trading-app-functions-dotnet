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

            user.gate = updatedUser.gate;
            user.crypto = updatedUser.crypto;
            user.coinbase = updatedUser.coinbase;
            user.bybit = updatedUser.bybit;
            user.binance = updatedUser.binance;

            var body = await this.tradingDbService.UpdateUserAsync(user);

            return new OkObjectResult(body);
        }
    }
}

