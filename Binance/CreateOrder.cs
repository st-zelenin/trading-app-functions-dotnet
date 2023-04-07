using System;
using System.Linq;
using System.Threading.Tasks;
using Binance.Interfaces;
using Binance.Models;
using Common.Interfaces;
using Common.Models;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using BinanceProduct = Binance.Models.Product;
using CommonProduct = Common.Models.Product;

namespace Binance;

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

        if (newOrder.type == BinanceOrderType.MARKET)
        {
            var newMarketOrderResult =
                await this.httpService.PostSignedAsync<BinanceOrder, NewMarketOrder>("/api/v3/order", (NewMarketOrder)newOrder);
            return new OkObjectResult(newMarketOrderResult);
        }

        var newLimitOrderResult =
            await this.httpService.PostSignedAsync<BinanceOrder, NewLimitOrder>("/api/v3/order", (NewLimitOrder)newOrder);
        return new OkObjectResult(newLimitOrderResult);
    }

    private async Task<BaseNewOrder> GetNewOrder(NewOrder order)
    {
        var product = await this.GetProductAsync(order.currencyPair);

        if (order.market == true)
        {
            return this.ToNewMarketOrder(order);
        }

        decimal price;
        if (!decimal.TryParse(order.price, out price))
        {
            throw new ArgumentException($"unexpected price value: {order.price}");
        }

        var ticker = await this.httpService.GetAsync<Models.Ticker, SymbolParams>("/api/v3/ticker/24hr", new SymbolParams { symbol = order.currencyPair });

        decimal tickerPrice;
        if (ticker == null || !decimal.TryParse(ticker.lastPrice, out tickerPrice))
        {
            return this.ToNewLimitOrder(order, price, product);
        }

        if (order.side == CommonOrderSides.buy && price > tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        if (order.side == CommonOrderSides.sell && price < tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        return this.ToNewLimitOrder(order, price, product);
    }


    private async Task<CommonProduct> GetProductAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("missing \"currencyPair\"");
        }

        var exchangeInfo =
            await this.httpService.GetAsync<ExchangeInfo<BinanceProduct>, SymbolParams>("/api/v3/exchangeInfo", new SymbolParams { symbol = name });

        return exchangeInfo.symbols.First().ToCommonProduct();
    }

    private NewMarketOrder ToNewMarketOrder(NewOrder order)
    {
        decimal amount;
        if (!decimal.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {amount}");
        }

        return new NewMarketOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY,
            quoteOrderQty = amount
        };
    }

    private NewLimitOrder ToNewLimitOrder(NewOrder order, decimal price, CommonProduct product)
    {
        decimal amount;
        if (!decimal.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {order.amount}");
        }

        return new NewLimitOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY,
            price = this.RoundToSmallestUnit(price, (decimal)product.pricePrecision),
            quantity = this.RoundToSmallestUnit(amount, (decimal)product.minQuantity)
        };
    }

    private decimal RoundToSmallestUnit(decimal num, decimal smallestUnit)
    {
        var x = Math.Round(1 / smallestUnit); // TODO: TEST!!! "0.00001"
        return Math.Round(num * x) / x;
    }
}

internal class SymbolParams
{
    public string symbol { get; set; }
}
