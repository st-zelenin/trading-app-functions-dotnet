using System;
using System.Net.Mime;
using ByBit.Interfaces;
using ByBit.Services;
using Common;
using Common.Interfaces;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ByBit.Startup))]

namespace ByBit;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<IEnvironmentVariableService, EnvironmentVariableService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISecretsService, SecretsService>();
        builder.Services.AddSingleton<ITradingDbService, TradingDbService>();
        builder.Services.AddSingleton<IByBitDbService, ByBitDbService>();
        builder.Services.AddSingleton<ITradeHistoryService, TradeHistoryService>();
        builder.Services.AddSingleton<IDexDbService, DexDbService>();

        builder.Services.AddScoped<IDexService, DexService>();

        builder.Services.AddTransient<IHttpService, HttpService>();

        builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
        {
            client.BaseAddress = new Uri("https://api.bybit.com");
            client.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
        });
    }
}

