using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common;
using Common.Interfaces;
using Crypto.Interfaces;
using Crypto.Services;
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
            builder.Services.AddSingleton<IDbService, DbService>();
            builder.Services.AddSingleton<IHttpService, HttpService>();

            //builder.Services.Configure<JsonSerializerOptions>(options =>
            //{
            //    //options.PropertyNameCaseInsensitive = true;


            //    options.Converters.Clear();
            //    options.Converters.Add(new JsonStringEnumConverter());

            //});


            



        }
    }
}