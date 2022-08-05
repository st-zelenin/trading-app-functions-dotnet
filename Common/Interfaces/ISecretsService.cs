using System;
namespace Common.Interfaces
{
    public interface ISecretsService
    {
        string GetSecret(string key);
    }
}

