using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using DataAccess.Models;
using Gate.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gate.Services
{
    internal class ImportPeriodTradeHistoryRequestData
    {
        public string currency_pair { get; set; }
        public string status { get; set; }
        public long from { get; set; }
        public long to { get; set; }
    }

    public class TradeHistoryService : ITradeHistoryService
    {
        private readonly IGateDbService gateDbService;
        private readonly IHttpService httpService;
        private readonly ILogger<ITradeHistoryService> log;

        public int historyDaysDiff { get; private set; }

        public TradeHistoryService(
            IGateDbService gateDbService,
            IHttpService httpService,
            ILogger<ITradeHistoryService> log)
        {
            this.gateDbService = gateDbService;
            this.httpService = httpService;
            this.log = log;
            this.historyDaysDiff = -29;
        }

        public Task UpdateRecentTradeHistory(string pair, string azureUserId)
        {
            return this.ImportPeriodTradeHistory(pair, DateTime.Now, DateTime.Now.AddDays(this.historyDaysDiff), azureUserId);
        }

        public async Task ImportPeriodTradeHistory(string pair, DateTime end, DateTime start, string azureUserId)
        {
            var data = new ImportPeriodTradeHistoryRequestData()
            {
                currency_pair = pair,
                status = "finished",
                to = new DateTimeOffset(end).ToUnixTimeSeconds(),
                from = new DateTimeOffset(start).ToUnixTimeSeconds()
            };

            var orders = await this.httpService.GetAsync<IEnumerable<GateOrder>, ImportPeriodTradeHistoryRequestData>
                ("/spot/orders", data);

            if (orders.Count() <= 0)
            {
                return;
            }

            await this.gateDbService.UpsertOrdersAsync(orders, azureUserId);
        }

        public async Task ImportTradeHistoryAsync(string pair, string azureUserId)
        {
            var end = DateTime.Now;
            var start = new DateTime(end.Ticks).AddDays(this.historyDaysDiff);

            var counter = 0;
            var tasks = new List<Task>();

            try
            {
                do
                {
                    counter++;
                    tasks.Add(this.ImportPeriodTradeHistory(pair, end, start, azureUserId));

                    end = new DateTime(start.Ticks);
                    start = new DateTime(end.Ticks).AddDays(this.historyDaysDiff);
                } while (counter < 24);

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                this.log.LogError($"failed to import.\nMessage = \"{ ex.Message}\"\nStackTrace = \"{ex.StackTrace}\"");
                throw;
            }
        }
    }
}
