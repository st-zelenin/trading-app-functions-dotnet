using System;
namespace Common
{
    public class EnvironmentVariableService
    {
        public static class Keys
        {
            public static string AzureKeyVaultEndpoint => "AzureKeyVaultEndpoint";
            public static string CosmosDbEndpoint => "CosmosDbEndpoint";
        }

        public static string GetVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}

