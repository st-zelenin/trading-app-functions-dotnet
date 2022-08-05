using System;
using Common;
using Common.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Crypto.Startup2))]

namespace Crypto
{
    public class Startup2 : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            //builder.Services.AddSingleton<IMyService>((s) => {
            //    return new MyService();
            //});

            builder.Services.AddSingleton<ISecretsService, SecretsService>();

            Console.WriteLine("----------------------");
        }
    }
}