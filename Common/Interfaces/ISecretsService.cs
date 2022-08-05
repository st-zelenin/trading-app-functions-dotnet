using System;
namespace Common.Interfaces
{
    public interface ISecretsService
    {
        Task<string> GetSecret(string key);
    }
}

