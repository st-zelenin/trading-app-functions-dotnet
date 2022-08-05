using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Common
{
    public class SecretsService
    {
        public static class Keys
        {
            public static string CosmosClient => "COSMOS-CLIENT-KEY";
        }

        public static string GetSecret(string key)
        {
            var vaultLocation = EnvironmentVariableService.GetVariable(EnvironmentVariableService.Keys.AzureKeyVaultEndpoint);
            var client = new SecretClient(new Uri(vaultLocation), credential: new DefaultAzureCredential());

            KeyVaultSecret secret = client.GetSecret(key);

            return secret.Value;
        }
    }
}

