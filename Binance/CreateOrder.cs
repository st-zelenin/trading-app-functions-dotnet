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

    private const string URL = "/api/v3/order";

    public CreateOrder(IHttpService httpService, IAuthService authService)
    {
        this.httpService = httpService;
        this.authService = authService;
    }

    [FunctionName("CreateOrder")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        var order = await this.httpService.GetRequestBody<NewOrder>(req) ?? throw new ArgumentException("\"order\" is missing");
        this.authService.ValidateUser(req);

        try
        {
            if (order.market == true)
            {
                return await this.CreateMarketOrder(order);
            }


            if (!decimal.TryParse(order.price, out decimal price))
            {
                throw new ArgumentException($"unexpected price value: {order.price}");
            }

            var ticker = await this.httpService.GetAsync<Models.Ticker, SymbolParams>("/api/v3/ticker/24hr", new SymbolParams { symbol = order.currencyPair });
            var product = await this.GetProductAsync(order.currencyPair);

            if (ticker == null || !decimal.TryParse(ticker.lastPrice, out decimal tickerPrice))
            {
                return await this.CreateLimitOrder(order, price, product);
            }

            if (order.side == CommonOrderSides.buy && price > tickerPrice)
            {
                return await this.CreateMarketOrder(order);
            }

            if (order.side == CommonOrderSides.sell && price < tickerPrice)
            {
                return await this.CreateMarketOrder(order);
            }

            return await this.CreateLimitOrder(order, price, product);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }

    private async Task<IActionResult> CreateMarketOrder(NewOrder order)
    {
        if (decimal.TryParse(order.total, out decimal total) && total > 0)
        {
            var newOrder = new NewMarketTotalMoneyOrder()
            {
                symbol = order.currencyPair,
                side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY,
                quoteOrderQty = total
            };

            var result = await this.httpService.PostSignedAsync<BinanceOrder, NewMarketTotalMoneyOrder>(URL, newOrder);

            return new OkObjectResult(result);
        }

        if (decimal.TryParse(order.amount, out decimal amount) && amount > 0)
        {
            var newOrder = new NewMarketCoinsQuantityOrder()
            {
                symbol = order.currencyPair,
                side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY,
                quantity = amount
            };

            var result = await this.httpService.PostSignedAsync<BinanceOrder, NewMarketCoinsQuantityOrder>(URL, newOrder);

            return new OkObjectResult(result);
        }

        throw new ArgumentException($"either amount or total should be provided");
    }

    private async Task<IActionResult> CreateLimitOrder(NewOrder order, decimal price, CommonProduct product)
    {
        var newOrder = ToNewLimitOrder(order, price, product);

        var result = await this.httpService.PostSignedAsync<BinanceOrder, NewLimitOrder>(URL, newOrder);

        return new OkObjectResult(result);
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

    private static NewLimitOrder ToNewLimitOrder(NewOrder order, decimal price, CommonProduct product)
    {
        if (!decimal.TryParse(order.amount, out decimal amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {order.amount}");
        }

        return new NewLimitOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY,
            price = RoundToSmallestUnit(price, (decimal)product.pricePrecision),
            quantity = RoundToSmallestUnit(amount, product.minQuantity)
        };
    }

    private static decimal RoundToSmallestUnit(decimal num, decimal smallestUnit)
    {
        var x = Math.Round(1 / smallestUnit); // TODO: TEST!!! "0.00001"
        return Math.Round(num * x) / x;
    }
}

internal class SymbolParams
{
    public string symbol { get; set; }
}
