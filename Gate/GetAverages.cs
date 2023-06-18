using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate
{
    public class GetAverages
    {
        private readonly IGateDbService gateDbService;
        private readonly IAuthService authService;

        public GetAverages(IGateDbService gateDbService, IAuthService authService)
        {
            this.gateDbService = gateDbService;
            this.authService = authService;
        }

        [FunctionName("GetAverages")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUserId = this.authService.GetUserId(req);

            var rawAverages = await this.gateDbService.GetAveragesAsync(azureUserId);

            var body = rawAverages.Aggregate(
                new Dictionary<string, Average>(),
                (acc, curr) =>
                {
                    if (!acc.TryGetValue(curr.currency_pair, out Average average))
                    {
                        average = new Average()
                        {
                            buy = new AverageSide() { money = 0, price = 0, volume = 0 },
                            sell = new AverageSide() { money = 0, price = 0, volume = 0 },
                        };

                        acc.Add(curr.currency_pair, average);
                    }

                    var side = curr.side == "buy" ? average.buy : average.sell;

                    side.money = curr.total_money;
                    side.volume = curr.total_volume;
                    side.price = curr.total_money / curr.total_volume;

                    return acc;
                });


            return new OkObjectResult(body);
        }
    }
}

