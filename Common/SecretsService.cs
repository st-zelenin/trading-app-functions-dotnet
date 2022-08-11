using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Common.Interfaces;
using Newtonsoft.Json;

namespace Common
{
    public static class SecretsKeys
    {
        public static string CosmosClient => "COSMOS-CLIENT-KEY";
        public static string CryptoApiKey => "CRYPTO-SPOT-TRADE";
        public static string ByBitApiKey => "BYBIT-SPOT";
    }

    public class SecretsService : ISecretsService
    {
        private readonly SecretClient client;
        //private readonly Dictionary<string, string> cache = new Dictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();


        public SecretsService(IEnvironmentVariableService environmentVariableService)
        {
            var vaultLocation = environmentVariableService.GetVariable(EnvironmentVariableKeys.AzureKeyVaultEndpoint);

            this.client = new SecretClient(new Uri(vaultLocation), new DefaultAzureCredential());
        }

        public async Task<string> GetSecretAsync(string key)
        {
            //string? secretValue;
            //if (this.cache.TryGetValue(key, out secretValue))
            //{
            //    return secretValue;
            //}

            KeyVaultSecret secret = await client.GetSecretAsync(key);
            this.cache.GetOrAdd(key, secret.Value);
            //this.cache.Add(key, secret.Value);

            return secret.Value;
        }

        public async Task<T> GetSecretAsync<T>(string key) where T : new()
        {
            //string? secretValue;
            //if (this.cache.TryGetValue(key, out secretValue))
            //{
            //    return JsonConvert.DeserializeObject<T>(secretValue);
            //}

            KeyVaultSecret secret = await client.GetSecretAsync(key);
            this.cache.GetOrAdd(key, secret.Value);
            //this.cache.Add(key, secret.Value);

            return JsonConvert.DeserializeObject<T>(secret.Value);
        }
    }
}

