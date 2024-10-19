using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Common.Interfaces;
using DataAccess.Interfaces;

namespace Binance;

public class GetAverages
{
    private readonly IAuthService authService;
    private readonly IBinanceDbService binanceDbService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;

    public GetAverages(IAuthService authService, IBinanceDbService binanceDbService, IDexDbService dexDbService, IDexService dexService)
    {
        this.authService = authService;
        this.binanceDbService = binanceDbService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.binanceDbService.GetAveragesAsync(azureUserId);
        var rawDexAverages = await this.dexDbService.GetAveragesAsync(azureUserId, "binance");

        var body = this.dexService.CombineCexWithDexAverages(rawCexAverages, rawDexAverages);

        return new OkObjectResult(body);
    }
}

