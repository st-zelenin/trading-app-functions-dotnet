using System;
using System.Threading.Tasks;
using Common.Interfaces;
using Gate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate
{
    internal class CancelOrderRequestData
    {
        public string id { get; set; }
        public string pair { get; set; }
    }

    internal class CancelOrderRequestParams
    {
        public string currency_pair { get; set; }
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

            if (data == null || data.id == null || data.pair == null)
            {
                throw new ArgumentNullException("id or pair is missing");
            }

            this.authService.ValidateUser(req);

            await this.httpService.DeleteAsync<object, CancelOrderRequestParams>($"/spot/orders/{data.id}", new CancelOrderRequestParams() { currency_pair = data.pair });

            return new OkResult();
        }
    }
}
