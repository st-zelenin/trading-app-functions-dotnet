using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gate;

public class GetAverages
{
    private readonly IGateDbService gateDbService;
    private readonly IAuthService authService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;

    public GetAverages(IGateDbService gateDbService, IAuthService authService, IDexDbService dexDbService, IDexService dexService)
    {
        this.gateDbService = gateDbService;
        this.authService = authService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.gateDbService.GetAveragesAsync(azureUserId);
        var rawDexAverages = await this.dexDbService.GetAveragesAsync(azureUserId, "gate");

        var body = this.dexService.CombineCexWithDexAverages(rawCexAverages, rawDexAverages);

        return new OkObjectResult(body);
    }
}

