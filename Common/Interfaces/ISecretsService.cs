namespace Common.Interfaces;

public interface ISecretsService
{
    Task<string> GetSecretAsync(string key);
    Task<T> GetSecretAsync<T>(string key) where T : new();
}

