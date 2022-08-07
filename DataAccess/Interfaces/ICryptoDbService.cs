using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface ICryptoDbService
    {
        Task<IEnumerable<CryptoAverage>> GetAveragesAsync(string containerId);
        Task<IEnumerable<CryptoOrder>> GetFilledOrdersAsync(string pair, string containerId);
    }
}

