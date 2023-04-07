using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using DataAccess.Interfaces;
using Common.Interfaces;

namespace User;

public class DoSomeTechService
{
    private readonly IAuthService authService;
    private readonly ITradingDbService tradingDbService;

    public DoSomeTechService(IAuthService authService, ITradingDbService tradingDbService)
    {
        this.authService = authService;
        this.tradingDbService = tradingDbService;
    }

    [FunctionName("DoSomeTechService")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        await tradingDbService.DoSomeTechService(azureUserId);

        return new OkResult();
    }
}

