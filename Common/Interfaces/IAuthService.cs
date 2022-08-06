using System;
namespace Common.Interfaces
{
    public interface IAuthService
    {
        string GetUserId(string authorizationHeader);
        void ValidateUser(string authorizationHeader);
    }
}

