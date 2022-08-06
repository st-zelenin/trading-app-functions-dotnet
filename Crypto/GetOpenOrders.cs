﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using CommonOrder = Common.Models.Order;

namespace Crypto
{
    public class GetOpenOrders
    {
        private readonly IAuthService authService;
        private readonly IHttpService httpService;

        public GetOpenOrders(IAuthService authService, IHttpService httpService)
        {
            this.authService = authService;
            this.httpService = httpService;
        }

        [FunctionName("GetOpenOrders")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            this.authService.ValidateUser(req.Headers["Authorization"]);

            var orders = new List<Order>();
            var page = 0;
            var done = false;

            do
            {
                var response = await this.httpService.PostAsync<ResponseWithResult<OrdersResponseResult>, GetOpenOrdersRequestBody>("private/get-open-orders", new GetOpenOrdersRequestBody() { page_size = 100, page = page });

                orders.AddRange(response.result.order_list);
                done = orders.Count >= response.result.count;
                page++;
            } while (!done);


            var body = orders.Aggregate(
                new Dictionary<string, IList<CommonOrder>>(),
                (acc, raw) =>
                {
                    IList<CommonOrder> instrument;

                    if (!acc.TryGetValue(raw.instrument_name, out instrument))
                    {
                        instrument = new List<CommonOrder>();
                        acc.Add(raw.instrument_name, instrument);
                    }

                    instrument.Add(raw.ToCommonOrder());

                    return acc;
                });


            return new OkObjectResult(body);
        }
    }


    internal class GetOpenOrdersRequestBody
    {
        public int page_size { get; set; }
        public int page { get; set; }
    }
}
