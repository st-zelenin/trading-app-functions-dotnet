using System;
using ByBit.Interfaces;
using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ByBit;

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

        var tasks = users.Aggregate(new List<Task>(), (acc, curr) =>
        {
            foreach (var pair in curr.bybit)
            {
                acc.Add(this.tradeHistoryService.UpdateRecentTradeHistoryAsync(pair.symbol, curr.id));
            }

            return acc;
        });

        await Task.WhenAll(tasks);

        log.LogInformation($"ByBit: UpdateRecentHistoryByTimer executed at: {DateTime.Now}");
    }
}

