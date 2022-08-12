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

namespace ByBit
{
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

            var side = order.side == CommonOrderSides.sell ? ByBitOrderSide.Sell : ByBitOrderSide.Buy;
            var product = await this.GetProductAsync(order.currencyPair);

            if (order.market == true)
            {
                var newMarketOrderResult =
                    await this.httpService.PostAsync("/spot/v1/order", this.ToNewMarketOrder(order, side));
                return new OkObjectResult(newMarketOrderResult);
            }

            var newLimitOrderResult = await this.httpService.PostAsync("/spot/v1/order", this.ToNewLimitOrder(order, side, product));
            return new OkObjectResult(newLimitOrderResult);
        }

        private async Task<ByBitProduct> GetProductAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("missing \"currencyPair\"");
            }

            var products = await this.httpService.GetAsync<ResponseWithResult<IEnumerable<ByBitProduct>>>("/spot/v1/symbols");

            return products.result.First((i) => i.name == name);
        }

        private NewMarketOrder ToNewMarketOrder(NewOrder order, ByBitOrderSide side)
        {
            double amount;
            if (!double.TryParse(order.amount, out amount) || amount <= 0)
            {
                throw new ArgumentException($"unexpected amount value: {amount}");
            }

            return new NewMarketOrder()
            {
                symbol = order.currencyPair,
                side = side,
                qty = amount
            };
        }

        private NewLimitOrder ToNewLimitOrder(NewOrder order, ByBitOrderSide side, ByBitProduct product)
        {
            double price;
            if (!double.TryParse(order.price, out price) || price <= 0)
            {
                throw new ArgumentException($"unexpected price value: {order.price}");
            }

            double amount;
            if (!double.TryParse(order.amount, out amount) || amount <= 0)
            {
                throw new ArgumentException($"unexpected amount value: {order.amount}");
            }

            double minPricePrecision;
            if (!double.TryParse(product.minPricePrecision, out minPricePrecision))
            {
                throw new ArgumentException($"unexpected minPricePrecision value: {product.minPricePrecision}");
            }

            double basePrecision;
            if (!double.TryParse(product.basePrecision, out basePrecision))
            {
                throw new ArgumentException($"unexpected basePrecision value: {product.basePrecision}");
            }

            return new NewLimitOrder()
            {
                symbol = order.currencyPair,
                side = side,
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
}

