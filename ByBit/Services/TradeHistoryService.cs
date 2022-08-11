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
            var data = new OrderHistoryRequestParams() { symbol = pair };

            return this.ImportTradeHistoryAsync(data, azureUserId);
        }

        public Task UpdateRecentTradeHistoryAsync(string pair, string azureUserId)
        {
            var data = new OrderHistoryRequestParamsWithLimit() { symbol = pair, limit = 100 };

            return this.ImportTradeHistoryAsync(data, azureUserId);
        }

        private async Task ImportTradeHistoryAsync<T>(T data, string azureUserId) where T: OrderHistoryRequestParams
        {
            var response = await this.httpService.GetAsync<ResponseWithResult<IEnumerable<ByBitOrder>>, OrderHistoryRequestParams>
                ("/spot/v1/history-orders", data);
            if (response.result.Count() <= 0)
            {
                return;
            }

            var filledOrders = response.result.Where(o => o.status == ByBitOrderStatus.FILLED);
            if (filledOrders.Count() == 0)
            {
                return;
            }

            foreach (var filledOrder in filledOrders)
            {
                filledOrder.id = filledOrder.orderId;
            }

            await this.bybitDbService.UpsertOrdersAsync(filledOrders, azureUserId);
        }
    }
}

