﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Crypto.Interfaces;
using Crypto.Models;
using DataAccess.Interfaces;

namespace Crypto.Services;

public class TradeHistoryService : ITradeHistoryService
{
    private readonly ICryptoDbService cryptoDbService;
    private readonly IHttpService httpService;

    public int historyHoursDiff { get; private set; }

    public TradeHistoryService(ICryptoDbService cryptoDbService, IHttpService httpService)
    {
        this.cryptoDbService = cryptoDbService;
        this.httpService = httpService;
        this.historyHoursDiff = -23;
    }

    public Task UpdateRecentTradeHistory(string azureUserId)
    {
        return this.ImportPeriodTradeHistory(DateTime.Now, DateTime.Now.AddHours(this.historyHoursDiff), azureUserId);
    }

    public async Task ImportPeriodTradeHistory(DateTime end, DateTime start, string azureUserId)
    {
        var data = new ImportPeriodTradeHistoryRequestData()
        {
            end_time = new DateTimeOffset(end).ToUnixTimeMilliseconds(),
            start_time = new DateTimeOffset(start).ToUnixTimeMilliseconds()
        };

        var response = await this.httpService.PostAsync<ResponseWithResult<OrdersResponseResult>, ImportPeriodTradeHistoryRequestData>
            ("private/get-order-history", data);

        if (response.result.data is null || response.result.data.Count() <= 0)
        {
            return;
        }

        var filledOrders = response.result.data.Where(o => o.status == DataAccess.Models.CryptoOrderStatus.FILLED);
        if (filledOrders.Count() == 0)
        {
            return;
        }

        foreach (var filledOrder in filledOrders)
        {
            filledOrder.id = filledOrder.order_id;
        }

        await this.cryptoDbService.UpsertOrdersAsync(filledOrders, azureUserId);
    }
}

internal class ImportPeriodTradeHistoryRequestData
{
    public long start_time { get; set; }
    public long end_time { get; set; }
}
