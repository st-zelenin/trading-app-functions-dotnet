using Common;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Services;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Crypto.Startup))]

namespace Crypto
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IEnvironmentVariableService, EnvironmentVariableService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISecretsService, SecretsService>();
            builder.Services.AddSingleton<ITradingDbService, TradingDbService>();
            builder.Services.AddSingleton<ICryptoDbService, CryptoDbService>();
            builder.Services.AddSingleton<IHttpService, HttpService>();
            builder.Services.AddSingleton<ITradeHistoryService, TradeHistoryService>();
        }
    }
}