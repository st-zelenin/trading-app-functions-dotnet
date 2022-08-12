using System;
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
    internal class CancelOrderRequestData
    {
        public string id { get; set; }
    }

    internal class CancelOrderRequestParams
    {
        public string orderId { get; set; }
    }

    public class CancelOrder
    {
        private readonly IHttpService httpService;
        private readonly IAuthService authService;

        public CancelOrder(IHttpService httpService, IAuthService authService)
        {
            this.httpService = httpService;
            this.authService = authService;
        }

        [FunctionName("CancelOrder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var data = await this.httpService.GetRequestBody<CancelOrderRequestData>(req);

            if (data == null || data.id == null)
            {
                throw new ArgumentNullException("id is missing");
            }

            this.authService.ValidateUser(req);

            var body = await this.httpService.DeleteAsync<BaseResponse, CancelOrderRequestParams>("/spot/v1/order",
                new CancelOrderRequestParams() { orderId = data.id });

            return new OkObjectResult(body);
        }
    }
}
