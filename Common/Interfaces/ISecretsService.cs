using System;
using Common.Models;

namespace Common.Interfaces
{
    public interface ISecretsService
    {
        Task<string> GetSecret(string key);
        Task<T> GetSecret<T>(string key) where T : new();
    }
}

