using System;
using DataAccess.Interfaces;
using DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Interfaces;
using System.Linq;

namespace Binance.Services;

internal class OrderHistoryRequestParams
{
    public string symbol { get; set; }
}

internal class OrderHistoryRequestParamsWithLimit : OrderHistoryRequestParams
{
    public int limit { get; set; }
}

public class TradeHistoryService : ITradeHistoryService
{
    private readonly IBinanceDbService binanceDbService;
    private readonly IHttpService httpService;

    public TradeHistoryService(
        IBinanceDbService bybitDbService,
        IHttpService httpService)
    {
        this.binanceDbService = bybitDbService;
        this.httpService = httpService;
    }

    public Task ImportTradeHistoryAsync(string pair, string azureUserId)
    {
        var data = new OrderHistoryRequestParams() { symbol = pair };

        return this.ImportTradeHistoryAsync(data, azureUserId);
    }

    public Task UpdateRecentTradeHistoryAsync(string pair, string azureUserId)
    {
        var data = new OrderHistoryRequestParamsWithLimit() { symbol = pair, limit = 100 };

        return this.ImportTradeHistoryAsync(data, azureUserId);
    }

    private async Task ImportTradeHistoryAsync<T>(T data, string azureUserId) where T : OrderHistoryRequestParams
    {
        var response = await this.httpService.GetSignedAsync<IEnumerable<BinanceOrder>, OrderHistoryRequestParams>
            ("/api/v3/allOrders", data);
        if (response.Count() <= 0)
        {
            return;
        }

        var filledOrders = response.Where(o => o.status == BinanceOrderStatus.FILLED);
        if (filledOrders.Count() == 0)
        {
            return;
        }

        foreach (var filledOrder in filledOrders)
        {
            filledOrder.id = filledOrder.orderId;
        }

        await this.binanceDbService.UpsertOrdersAsync(filledOrders, azureUserId);
    }
}