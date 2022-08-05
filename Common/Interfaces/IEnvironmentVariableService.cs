using System;
namespace Common.Interfaces
{
    public interface IEnvironmentVariableService
    {
        string GetVariable(string key);
    }
}

