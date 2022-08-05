using System;
using Microsoft.Azure.Cosmos;

namespace Common.Interfaces
{
    public interface IDbService
    {
        Container GetUsersContainer();
    }
}

