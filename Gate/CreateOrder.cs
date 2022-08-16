using System;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Gate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using GateNewOrder = Gate.Models.NewOrder;

namespace Gate
{
    public class CreateOrder
    {
        private readonly IHttpService httpService;
        private readonly IAuthService authService;

        public CreateOrder(IHttpService httpService, IAuthService authService)
        {
            this.httpService = httpService;
            this.authService = authService;
        }

        [FunctionName("CreateOrder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var order = await this.httpService.GetRequestBody<NewOrder>(req);
            if (order == null)
            {
                throw new ArgumentException("\"order\" is missing");
            }

            this.authService.ValidateUser(req);

            await this.httpService.PostAsync<object, GateNewOrder>("/spot/orders", GateNewOrder.FromCommonNewOrder(order));

            return new OkResult();
        }
    }
}
