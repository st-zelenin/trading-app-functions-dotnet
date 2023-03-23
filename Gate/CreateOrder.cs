using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Models;
using Gate.Interfaces;
using Gate.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;

internal class GetTickerParams
{
    public string currency_pair { get; set; }
}

public class CreateOrder
{
    private readonly IHttpService httpService;
    private readonly IAuthService authService;

    public CreateOrder(IHttpService httpService, IAuthService authService)
    {
        this.httpService = httpService;
        this.authService = authService;
    }

    [FunctionName("CreateOrder")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        var order = await this.httpService.GetRequestBody<NewOrder>(req);
        if (order == null)
        {
            throw new ArgumentException("\"order\" is missing");
        }

        this.authService.ValidateUser(req);

        var newOrder = await this.GetNewOrder(order);

        var body = await this.httpService.PostAsync<GateOrder, BaseNewOrder>("/spot/orders", newOrder);

        return new OkObjectResult(body);
    }

    private async Task<BaseNewOrder> GetNewOrder(NewOrder order)
    {
        if (order.market == true)
        {
            return this.ToNewMarketOrder(order);
        }

        double price;
        if (!double.TryParse(order.price, out price))
        {
            throw new ArgumentException($"unexpected price value: {order.price}");
        }

        var tickers = await this.httpService.GetAsync<IEnumerable<Models.Ticker>, GetTickerParams>("/spot/tickers", new GetTickerParams { currency_pair = order.currencyPair });

        double tickerPrice;
        if (tickers == null || tickers.Count() == 0 || !double.TryParse(tickers.First().last, out tickerPrice))
        {
            return this.ToNewLimitOrder(order, price);
        }

        if (order.side == CommonOrderSides.buy && price > tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        if (order.side == CommonOrderSides.sell && price < tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        return this.ToNewLimitOrder(order, price);
    }

    private NewMarketOrder ToNewMarketOrder(NewOrder order)
    {
        if (order.side == CommonOrderSides.buy && string.IsNullOrEmpty(order.total))
        {
            throw new ArgumentException($"total should be provided for buying by market");
        }

        if (order.side == CommonOrderSides.sell && string.IsNullOrEmpty(order.amount))
        {
            throw new ArgumentException($"amount should be provided for selling by market");
        }

        return new NewMarketOrder
        {
            currency_pair = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? GateOrderSide.sell : GateOrderSide.buy,
            amount = order.side == CommonOrderSides.buy ? order.total : order.amount
        };
    }

    private NewLimitOrder ToNewLimitOrder(NewOrder order, double price)
    {
        return new NewLimitOrder
        {
            currency_pair = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? GateOrderSide.sell : GateOrderSide.buy,
            amount = order.amount,
            price = price,
        };
    }
}
