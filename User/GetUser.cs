using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Common.Models;

namespace User
{
    public class GetUser
    {
        private readonly IAuthService authService;
        private readonly ITradingDbService tradingDbService;

        public GetUser(IAuthService authService, ITradingDbService tradingDbService)
        {
            this.authService = authService;
            this.tradingDbService = tradingDbService;
        }

        [FunctionName("GetUser")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUser = this.authService.GetAzureUser(req);

            //azureUser.oid += "000111222333";

            var user = await this.tradingDbService.GetOrCreateUserAsync(azureUser);

            return new OkObjectResult(user);
        }
    }
}

