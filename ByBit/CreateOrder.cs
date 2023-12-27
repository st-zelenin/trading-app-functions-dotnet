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
using Newtonsoft.Json;
using ByBitProduct = ByBit.Models.Product;

namespace ByBit;

internal class CreateOrderResponce
{
    public string orderId { get; set; }
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

        if (order.market == true)
        {
            return await this.CreateMarketOrder(order);
        }

        double price;
        if (!double.TryParse(order.price, out price))
        {
            throw new ArgumentException($"unexpected price value: {order.price}");
        }

        var tickers = await this.httpService.GetUnsignedAsync<ResponseWithListResult_V5<Models.Ticker>, SpotSymbolParams>("/v5/market/tickers", new SpotSymbolParams { symbol = order.currencyPair });

        double tickerPrice;
        if (tickers.result.list == null || tickers.result.list.Count() == 0 || !double.TryParse(tickers.result.list.First().lastPrice, out tickerPrice))
        {
            return await this.CreateLimitOrder(order, price);
        }

        if (order.side == CommonOrderSides.buy && price > tickerPrice)
        {
            return await this.CreateMarketOrder(order);
        }

        if (order.side == CommonOrderSides.sell && price < tickerPrice)
        {
            return await this.CreateMarketOrder(order);
        }

        return await this.CreateLimitOrder(order, price);
    }

    private async Task<IActionResult> CreateMarketOrder(NewOrder order)
    {
        var newOrder = this.ToNewMarketOrder(order);
        var serializedBody = JsonConvert.SerializeObject(newOrder);

        var newOrderResult =
            await this.httpService.PostAsync<ResponseWithResult<CreateOrderResponce>>("/v5/order/create", serializedBody);
        return new OkObjectResult(newOrderResult);
    }

    private async Task<IActionResult> CreateLimitOrder(NewOrder order, double price)
    {
        var product = await this.GetProductAsync(order.currencyPair);

        var newOrder = this.ToNewLimitOrder(order, product, price);
        var serializedBody = JsonConvert.SerializeObject(newOrder);

        var newOrderResult =
            await this.httpService.PostAsync<ResponseWithResult<CreateOrderResponce>>("/v5/order/create", serializedBody);
        return new OkObjectResult(newOrderResult);
    }

    private async Task<ByBitProduct> GetProductAsync(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            throw new ArgumentException("missing \"currencyPair\"");
        }

        var response = await this.httpService.GetUnsignedAsync<ResponseWithListResult_V5<Models.Product>, SpotSymbolParams>("/v5/market/instruments-info", new SpotSymbolParams { symbol = symbol });

        return response.result.list.First();
    }

    private NewMarketOrder ToNewMarketOrder(NewOrder order)
    {
        double total;
        if (!double.TryParse(order.total, out total) || total <= 0)
        {
            throw new ArgumentException($"unexpected total value: {total}");
        }

        return new NewMarketOrder()
        {
            symbol = order.currencyPair,
            side = order.side == CommonOrderSides.sell ? ByBitOrderSide.Sell : ByBitOrderSide.Buy,
            qty = total.ToString()
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
            price = this.GetRoundedDecimalString(price, minPricePrecision),
            qty = this.GetRoundedDecimalString(amount, basePrecision)
        };
    }

    private string GetRoundedDecimalString(double num, double smallestUnit)
    {
        var precision = Math.Round(1 / smallestUnit); // TODO: TEST!!! "0.00001"
        var rounded = Math.Round(num * precision) / precision;
        return rounded.ToString();
    }
}
