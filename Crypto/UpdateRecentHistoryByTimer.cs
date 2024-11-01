using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crypto.Interfaces;
using DataAccess.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Crypto;

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

        List<Task> tasks = new();
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

