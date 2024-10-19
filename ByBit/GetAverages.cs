using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ByBit;

public class GetAverages
{
    private readonly IByBitDbService bybitDbService;
    private readonly IAuthService authService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;

    public GetAverages(IByBitDbService bybitDbService, IAuthService authService, IDexDbService dexDbService, IDexService dexService)
    {
        this.bybitDbService = bybitDbService;
        this.authService = authService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.bybitDbService.GetAveragesAsync(azureUserId);
        var rawDexAverages = await this.dexDbService.GetAveragesAsync(azureUserId, "bybit");

        var body = this.dexService.CombineCexWithDexAverages(rawCexAverages, rawDexAverages);

        return new OkObjectResult(body);
    }
}

