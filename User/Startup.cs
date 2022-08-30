using Common;
using Common.Interfaces;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(User.Startup))]

namespace User
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

            builder.Services.AddSingleton<IBaseHttpService, BaseHttpService>();
        }
    }
}