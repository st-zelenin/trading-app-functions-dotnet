using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Crypto.Interfaces;
using Common.Interfaces;

namespace Crypto
{
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
            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var data = JsonConvert.DeserializeObject<CancelOrderRequestData>(requestBody);
            if (data.id == null || data.pair == null)
            {
                throw new ArgumentNullException("id or pair is missing");
            }

            this.authService.ValidateUser(req.Headers["Authorization"]);

            var body = await this.httpService.PostAsync("private/cancel-order", new { order_id = data.id, instrument_name = data.pair });

            return new OkObjectResult(body);
        }
    }

    internal class CancelOrderRequestData
    {
        public string id { get; set; }
        public string pair { get; set; }
    }
}

