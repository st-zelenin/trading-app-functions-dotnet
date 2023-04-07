using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using Common.Interfaces;
using Common.Models;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using ByBitProduct = ByBit.Models.Product;

namespace ByBit;

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

        var product = await this.GetProductAsync(order.currencyPair);
        var newOrder = await this.GetNewOrder(order, product);

        if (newOrder.type == ByBitOrderType.MARKET)
        {
            var newMarketOrderResult =
                await this.httpService.PostAsync("/spot/v1/order", (NewMarketOrder)newOrder);
            return new OkObjectResult(newMarketOrderResult);
        }

        var newLimitOrderResult = await this.httpService.PostAsync("/spot/v1/order", (NewLimitOrder)newOrder);
        return new OkObjectResult(newLimitOrderResult);
    }

    private async Task<ByBitProduct> GetProductAsync(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            throw new ArgumentException("missing \"currencyPair\"");
        }

        var response = await this.httpService.GetAsync<ResponseWithListResult_V5<Models.Product>, SpotSymbolParams>("/v5/market/instruments-info", new SpotSymbolParams { symbol = symbol });

        return response.result.list.First();
    }

    private async Task<BaseNewOrder> GetNewOrder(NewOrder order, ByBitProduct product)
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

        var response = await this.httpService.GetAsync<ResponseWithListResult_V5<Models.Ticker>, SpotSymbolParams>("/v5/market/tickers", new SpotSymbolParams { symbol = order.currencyPair });

        double tickerPrice;
        if (response.result.list == null || response.result.list.Count() == 0 || !double.TryParse(response.result.list.First().lastPrice, out tickerPrice))
        {
            return this.ToNewLimitOrder(order, product, price);
        }

        if (order.side == CommonOrderSides.buy && price > tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        if (order.side == CommonOrderSides.sell && price < tickerPrice)
        {
            return this.ToNewMarketOrder(order);
        }

        return this.ToNewLimitOrder(order, product, price);
    }


    private NewMarketOrder ToNewMarketOrder(NewOrder order)
    {
        double amount;
        if (!double.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {amount}");
        }

        return new NewMarketOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? ByBitOrderSide.Sell : ByBitOrderSide.Buy,
            qty = amount
        };
    }

    private NewLimitOrder ToNewLimitOrder(NewOrder order, ByBitProduct product, double price)
    {
        double amount;
        if (!double.TryParse(order.amount, out amount) || amount <= 0)
        {
            throw new ArgumentException($"unexpected amount value: {order.amount}");
        }

        double minPricePrecision;
        if (!double.TryParse(product.priceFilter.tickSize, out minPricePrecision))
        {
            throw new ArgumentException($"unexpected minPricePrecision value: {product.priceFilter.tickSize}");
        }

        double basePrecision;
        if (!double.TryParse(product.lotSizeFilter.basePrecision, out basePrecision))
        {
            throw new ArgumentException($"unexpected basePrecision value: {product.lotSizeFilter.basePrecision}");
        }

        return new NewLimitOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? ByBitOrderSide.Sell : ByBitOrderSide.Buy,
            price = this.RoundToSmallestUnit(price, minPricePrecision),
            qty = this.RoundToSmallestUnit(amount, basePrecision)
        };
    }

    private double RoundToSmallestUnit(double num, double smallestUnit)
    {
        var x = Math.Round(1 / smallestUnit); // TODO: TEST!!! "0.00001"
        return Math.Round(num * x) / x;
    }
}
