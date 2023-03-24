using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Interfaces;
using Crypto.Interfaces;
using DataAccess.Interfaces;

namespace Crypto;

public class DoSomeTechService
{
    private readonly IHttpService httpService;
    private readonly IAuthService authService;
    private readonly ICryptoDbService cryptoDbService;

    public DoSomeTechService(IHttpService httpService, IAuthService authService, ICryptoDbService cryptoDbService)
    {
        this.httpService = httpService;
        this.authService = authService;
        this.cryptoDbService = cryptoDbService;
    }

    [FunctionName("DoSomeTechService")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var azureUserId = this.authService.GetUserId(req);

        await cryptoDbService.DoSomeTechService(azureUserId);

        return new OkResult();
    }
}

