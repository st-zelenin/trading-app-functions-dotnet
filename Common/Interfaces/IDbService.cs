using System;
using Common.Models;

namespace Common.Interfaces
{
    public interface IDbService
    {
        Task<Trader> GetUser(string azureUserId);
    }
}

