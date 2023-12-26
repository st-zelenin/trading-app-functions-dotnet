using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByBit.Interfaces;
using ByBit.Models;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace ByBit.Services
{
    internal class OrderHistoryRequestParams
    {
        public string symbol { get; set; }
        public string category { get; set; }
    }

    internal class OrderHistoryRequestParamsWithLimit : OrderHistoryRequestParams
    {
        public int limit { get; set; }
    }

    public class TradeHistoryService : ITradeHistoryService
    {
        private readonly IByBitDbService bybitDbService;
        private readonly IHttpService httpService;

        public TradeHistoryService(
            IByBitDbService bybitDbService,
            IHttpService httpService)
        {
            this.bybitDbService = bybitDbService;
            this.httpService = httpService;
        }

        public Task ImportTradeHistoryAsync(string pair, string azureUserId)
        {
            var data = new OrderHistoryRequestParams() { symbol = pair, category = "spot" };

            return this.ImportTradeHistoryAsync(data, azureUserId);
        }

        public Task UpdateRecentTradeHistoryAsync(string pair, string azureUserId)
        {
            var data = new OrderHistoryRequestParamsWithLimit() { symbol = pair, category = "spot", limit = 100 };

            return this.ImportTradeHistoryAsync(data, azureUserId);
        }

        private async Task ImportTradeHistoryAsync<T>(T data, string azureUserId) where T : OrderHistoryRequestParams
        {
            var response = await this.httpService.GetV5Async<ResponseWithListResult_V5<OrderV5>, OrderHistoryRequestParams>
                ("/v5/order/history", data);

            if (response.result.list.Count() <= 0)
            {
                return;
            }

            var filledOrders = response.result.list.Where(o => o.orderStatus == ByBitV5OrderStatus.Filled || (o.orderType == ByBitV5OrderType.Market && o.orderStatus == ByBitV5OrderStatus.PartiallyFilledCanceled) );
            if (filledOrders.Count() == 0)
            {
                return;
            }

            var convertedOrders = filledOrders.Select(o => o.ToByBitOrder());

            await this.bybitDbService.UpsertOrdersAsync(convertedOrders, azureUserId);
        }
    }
}

