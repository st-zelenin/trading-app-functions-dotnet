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
        private IEnvironmentVariableService environmentVariableService;

        public SecretsService(IEnvironmentVariableService environmentVariableService)
        {
            this.environmentVariableService = environmentVariableService;
        }

        public string GetSecret(string key)
        {
            var vaultLocation = this.environmentVariableService.GetVariable(EnvironmentVariableKeys.AzureKeyVaultEndpoint);
            var client = new SecretClient(new Uri(vaultLocation), credential: new DefaultAzureCredential());

            KeyVaultSecret secret = client.GetSecret(key);

            return secret.Value;
        }
    }
}

