using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Common.Interfaces;

namespace Common
{
    public static class SecretsKeys
    {
        public static string CosmosClient => "COSMOS-CLIENT-KEY";
    }

    public class SecretsService : ISecretsService
    {
        private readonly SecretClient client;
        private readonly Dictionary<string, string>  cache = new Dictionary<string, string>();

        public SecretsService(IEnvironmentVariableService environmentVariableService)
        {
            var vaultLocation = environmentVariableService.GetVariable(EnvironmentVariableKeys.AzureKeyVaultEndpoint);

            this.client = new SecretClient(new Uri(vaultLocation), new DefaultAzureCredential());
        }

        public async Task<string> GetSecret(string key)
        {
            string? secretValue;
            if (this.cache.TryGetValue(key, out secretValue))
            {
                return secretValue;
            }

            KeyVaultSecret secret = await client.GetSecretAsync(key);
            secretValue = secret.Value;
            this.cache.Add(key, secretValue);

            return secretValue;
        }
    }
}

