using Common.Models;

namespace Common.Interfaces;

public interface IDexService
{
    List<Order> CombineCexWithDexOrders(IEnumerable<Order> cex, IEnumerable<Order> dex);
    Dictionary<string, Average> CombineCexWithDexAverages(IEnumerable<CryptoAverage> cex, IEnumerable<CryptoAverage> dex);
}

