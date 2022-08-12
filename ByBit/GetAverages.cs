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

namespace ByBit
{
    public class GetAverages
    {
        private readonly IByBitDbService bybitDbService;
        private readonly IAuthService authService;

        public GetAverages(IByBitDbService bybitDbService, IAuthService authService)
        {
            this.bybitDbService = bybitDbService;
            this.authService = authService;
        }

        [FunctionName("GetAverages")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var azureUserId = this.authService.GetUserId(req);

            var rawAverages = await this.bybitDbService.GetAveragesAsync(azureUserId);

            var body = rawAverages.Aggregate(
                new Dictionary<string, Average>(),
                (acc, curr) =>
                {
                    Average average;

                    if (!acc.TryGetValue(curr.currency_pair, out average))
                    {
                        average = new Average()
                        {
                            buy = new AverageSide() { money = 0, price = 0, volume = 0 },
                            sell = new AverageSide() { money = 0, price = 0, volume = 0 },
                        };

                        acc.Add(curr.currency_pair, average);
                    }

                    var side = curr.side == "BUY" ? average.buy : average.sell;

                    side.money = curr.total_money;
                    side.volume = curr.total_volume;
                    side.price = curr.total_money / curr.total_volume;

                    return acc;
                });


            return new OkObjectResult(body);
        }
    }
}

