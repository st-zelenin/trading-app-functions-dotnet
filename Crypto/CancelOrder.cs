using System;
using System.IO;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Crypto
{
    internal class CancelOrderRequestData
    {
        public string id { get; set; }
        public string pair { get; set; }
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

            var body = await this.httpService.PostAsync("private/cancel-order", new { order_id = data.id, instrument_name = data.pair });

            return new OkObjectResult(body);
        }
    }
}

