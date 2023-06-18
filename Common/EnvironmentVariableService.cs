using Common.Interfaces;

namespace Common;

public static class EnvironmentVariableKeys
{
    public static string AzureKeyVaultEndpoint => "AzureKeyVaultEndpoint";
    public static string CosmosDbEndpoint => "CosmosDbEndpoint";
}

public class EnvironmentVariableService : IEnvironmentVariableService
{
    public string GetVariable(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }
}

