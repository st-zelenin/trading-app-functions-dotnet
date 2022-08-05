using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Common.Interfaces;

namespace Common
{
    public static class SecretsKeys
    {
        public static string CosmosClient => "COSMOS-CLIENT-KEY";
    }

    public class SecretsService: ISecretsService
    {
        public string GetSecret(string key)
        {
            var vaultLocation = EnvironmentVariableService.GetVariable(EnvironmentVariableService.Keys.AzureKeyVaultEndpoint);
            var client = new SecretClient(new Uri(vaultLocation), credential: new DefaultAzureCredential());

            KeyVaultSecret secret = client.GetSecret(key);

            return secret.Value;
        }
    }
}

