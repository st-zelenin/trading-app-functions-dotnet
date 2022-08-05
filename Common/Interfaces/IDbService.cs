using System;
using Microsoft.Azure.Cosmos;

namespace Common.Interfaces
{
    public interface IDbService
    {
        Task<Container> GetUsersContainer();
    }
}

