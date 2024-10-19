using System.Threading.Tasks;
using Common.Interfaces;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Crypto;

public class GetAverages
{
    private readonly ICryptoDbService cryptoDbService;
    private readonly IAuthService authService;
    private readonly IDexDbService dexDbService;
    private readonly IDexService dexService;

    public GetAverages(ICryptoDbService cryptoDbService, IAuthService authService, IDexDbService dexDbService, IDexService dexService)
    {
        this.cryptoDbService = cryptoDbService;
        this.authService = authService;
        this.dexDbService = dexDbService;
        this.dexService = dexService;
    }

    [FunctionName("GetAverages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        var rawCexAverages = await this.cryptoDbService.GetAveragesAsync(azureUserId);
        var rawDexAverages = await this.dexDbService.GetAveragesAsync(azureUserId, "crypto");

        var body = this.dexService.CombineCexWithDexAverages(rawCexAverages, rawDexAverages);

        return new OkObjectResult(body);
    }
}

