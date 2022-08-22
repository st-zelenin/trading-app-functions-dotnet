using Common;
using Common.Interfaces;
using Gate.Interfaces;
using Gate.Services;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Gate.Startup))]

namespace Gate
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddSingleton<IEnvironmentVariableService, EnvironmentVariableService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISecretsService, SecretsService>();
            builder.Services.AddSingleton<ITradingDbService, TradingDbService>();
            builder.Services.AddSingleton<IGateDbService, GateDbService>();

            builder.Services.AddTransient<IHttpService, HttpService>();
            builder.Services.AddSingleton<ITradeHistoryService, TradeHistoryService>();

            builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }
    }
}