using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Interfaces;
using Common.Models;
using DataAccess.Models;
using System.Collections.Generic;
using Binance.Interfaces;
using BinanceProduct = Binance.Models.Product;
using CommonProduct = Common.Models.Product;
using Binance.Models;
using Microsoft.Azure.Cosmos;
using System.Linq;

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

        var side = order.side == CommonOrderSides.sell ? BinanceOrderSide.SELL : BinanceOrderSide.BUY;
        var product = await this.GetProductAsync(order.currencyPair);

        if (order.market == true)
        {
            var newMarketOrderResult =
                await this.httpService.PostSignedAsync<BinanceOrder, NewMarketOrder>("/api/v3/order", this.ToNewMarketOrder(order, side));
            return new OkObjectResult(newMarketOrderResult);
        }

        var newLimitOrderResult =
            await this.httpService.PostSignedAsync<BinanceOrder, NewLimitOrder>("/api/v3/order", this.ToNewLimitOrder(order, side, product));
        return new OkObjectResult(newLimitOrderResult);
    }

    private async Task<CommonProduct> GetProductAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("missing \"currencyPair\"");
        }

        var exchangeInfo =
            await this.httpService.GetAsync<ExchangeInfo<BinanceProduct>, GetProductParams>("/api/v3/exchangeInfo", new GetProductParams { symbol = name });

        return exchangeInfo.symbols.First().ToCommonProduct();
    }

    private NewMarketOrder ToNewMarketOrder(NewOrder order, BinanceOrderSide side)
    {
        decimal amount;
        if (!decimal.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {amount}");
        }

        return new NewMarketOrder()
        {
            symbol = order.currencyPair,
            side = side,
            quoteOrderQty = amount
        };
    }

    private NewLimitOrder ToNewLimitOrder(NewOrder order, BinanceOrderSide side, CommonProduct product)
    {
        decimal price;
        if (!decimal.TryParse(order.price, out price) || price <= 0)
        {
            throw new ArgumentException($"unexpected price value: {order.price}");
        }

        decimal amount;
        if (!decimal.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {order.amount}");
        }

        return new NewLimitOrder()
        {
            symbol = order.currencyPair,
            side = side,
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

internal class GetProductParams
{
    public string symbol { get; set; }
}