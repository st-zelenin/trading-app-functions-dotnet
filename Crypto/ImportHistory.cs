using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Interfaces;
using Crypto.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Crypto;

internal class ImportHistoryStartRequestData
{
    public int periodMonths { get; set; }
}

internal class ImportHistoryOrchestratorInput: ImportHistoryStartRequestData
{
    public string azureUserId { get; set; }
}

public class ImportHistoryActivityInput
{
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public string azureUserId { get; set; }
}

public class ImportHistory
{
    private readonly IAuthService authService;
    private readonly ITradeHistoryService tradeHistoryService;

    public ImportHistory(
        IAuthService authService,
        ITradeHistoryService tradeHistoryService)
    {
        this.authService = authService;
        this.tradeHistoryService = tradeHistoryService;
    }

    [FunctionName("ImportHistory")]
    public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var input = context.GetInput<ImportHistoryOrchestratorInput>();

        var now = DateTime.Now;
        var end = new DateTime(now.Ticks);
        var start = new DateTime(now.Ticks).AddHours(this.tradeHistoryService.historyHoursDiff);

        var periodEndDate = new DateTime(now.Ticks).AddMonths(-input.periodMonths);

        do
        {
            // https://exchange-docs.crypto.com/spot/index.html#rate-limits
            await Task.Delay(TimeSpan.FromSeconds(1));

            await context.CallActivityAsync<string>("ImportHistory_ImportPeriod",
                new ImportHistoryActivityInput() { azureUserId = input.azureUserId, end = end, start = start });

            end = new DateTime(start.Ticks);
            start = new DateTime(end.Ticks).AddHours(this.tradeHistoryService.historyHoursDiff);
        } while (end.Ticks > periodEndDate.Ticks);
    }

    [FunctionName("ImportHistory_ImportPeriod")]
    public async Task SayHello([ActivityTrigger] ImportHistoryActivityInput input, ILogger log)
    {
        //try
        //{
        var requestStart = DateTime.Now;

        await this.tradeHistoryService.ImportPeriodTradeHistory(input.end, input.start, input.azureUserId);

        //var waitTicks = 1000 * TimeSpan.TicksPerMillisecond - (DateTime.Now - requestStart).Ticks;

        //Console.WriteLine($"waitTicks = {waitTicks}, executed = {(DateTime.Now - requestStart).Ticks}");

        //if (waitTicks > 0)
        //{
        //    await Task.Delay(TimeSpan.FromTicks(waitTicks));
        //}

        //// https://exchange-docs.crypto.com/spot/index.html#rate-limits
        //await Task.Delay(TimeSpan.FromSeconds(1));
        //}
        //catch (TooManyRequestsException)
        //{
        //    log.LogWarning("TooManyRequestsException - retry start");

        //    // TODO: play around with the delay
        //    await Task.Delay(TimeSpan.FromSeconds(10));

        //    await this.tradeHistoryService.ImportPeriodTradeHistory(input.end, input.start, input.azureUserId);

        //    log.LogWarning("TooManyRequestsException - retry success");
        //}
    }

    [FunctionName("ImportHistory_HttpStart")]
    public async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter)
    {
        var data = await req.Content.ReadAsAsync<ImportHistoryStartRequestData>();

        var azureUserId = this.authService.GetUserId(req);

        string instanceId = await starter.StartNewAsync("ImportHistory",
            new ImportHistoryOrchestratorInput() { azureUserId = azureUserId, periodMonths = data.periodMonths });

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}
