﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Crypto.Interfaces;
using Crypto.Models;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto
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

            var side = order.side == CommonOrderSides.sell ? CryptoOrderSide.SELL : CryptoOrderSide.BUY;

            var instrument = await this.GetInstrument(order.currencyPair);
            if (instrument == null)
            {
                throw new Exception($"failed to find instrument: {order.currencyPair}");
            }

            if (order.market == true)
            {
                var newMarketOrderResult =
                    await this.httpService.PostAsync("private/create-order", this.ToNewMarketOrder(order, side, instrument));
                return new OkObjectResult(newMarketOrderResult);
            }

            var newLimitOrderResult = await this.httpService.PostAsync("private/create-order", this.ToNewLimitOrder(order, side, instrument));
            return new OkObjectResult(newLimitOrderResult);
        }

        private async Task<Instrument> GetInstrument(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("missing \"currencyPair\"");
            }

            var instruments = await this.httpService.GetAsync<ResponseWithResult<InstrumentsResponseResult>>("public/get-instruments");
            return instruments.result.instruments.FirstOrDefault((i) => i.instrument_name == name);
        }

        private NewMarketOrder ToNewMarketOrder(NewOrder order, CryptoOrderSide side, Instrument instrument)
        {
            double amount;

            // if 'quantity' is here - no matter if it is BUY or SELL
            if (double.TryParse(order.amount, out amount) || amount > 0)
            {
                return new NewMarketOrder()
                {
                    instrument_name = order.currencyPair,
                    side = side,
                    quantity = Math.Round(amount, instrument.quantity_decimals)
                };
            }

            if (side == CryptoOrderSide.SELL)
            {
                throw new ArgumentException("a market SELL order must have \"quantity\"");
            }

            double total;
            if (!double.TryParse(order.total, out total) || total <= 0)
            {
                throw new ArgumentException("a market BUY order must have either \"quantity\" or \"notional\"");
            }

            return new NewMarketOrder()
            {
                instrument_name = order.currencyPair,
                side = side,
                notional = total
            };
        }

        private NewLimitOrder ToNewLimitOrder(NewOrder order, CryptoOrderSide side, Instrument instrument)
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

            return new NewLimitOrder()
            {
                instrument_name = order.currencyPair,
                side = side,
                price = Math.Round(price, instrument.price_decimals),
                quantity = Math.Round(amount, instrument.quantity_decimals)
            };
        }
    }
}

