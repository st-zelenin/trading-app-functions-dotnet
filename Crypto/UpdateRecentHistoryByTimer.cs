using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crypto.Interfaces;
using DataAccess.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Crypto
{
    public class UpdateRecentHistoryByTimer
    {
        private readonly ITradingDbService tradingDbService;
        private readonly ITradeHistoryService tradeHistoryService;

        public UpdateRecentHistoryByTimer(
            ITradingDbService tradingDbService,
            ITradeHistoryService tradeHistoryService)
        {
            this.tradingDbService = tradingDbService;
            this.tradeHistoryService = tradeHistoryService;
        }

        [FunctionName("UpdateRecentHistoryByTimer")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var users = await this.tradingDbService.GetUsersAsync();

            var tasks = new List<Task>();
            foreach (var user in users)
            {
                tasks.Add(this.tradeHistoryService.UpdateRecentTradeHistory(user.id));

                // https://exchange-docs.crypto.com/spot/index.html#rate-limits
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await Task.WhenAll(tasks);

            log.LogInformation($"Crypto: UpdateRecentHistoryByTimer executed at: {DateTime.Now}");
        }
    }
}

