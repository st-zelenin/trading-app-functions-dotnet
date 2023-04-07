﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Gate.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Gate;

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
            foreach (var pair in curr.gate)
            {
                acc.Add(this.tradeHistoryService.UpdateRecentTradeHistory(pair.symbol, curr.id));
            }

            return acc;
        });

        await Task.WhenAll(tasks);

        log.LogInformation($"Gate: UpdateRecentHistoryByTimer executed at: {DateTime.Now}");
    }
}

