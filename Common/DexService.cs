using Common.Interfaces;
using Common.Models;

namespace Common;

public class DexService : IDexService
{
    public Dictionary<string, Average> CombineCexWithDexAverages(IEnumerable<CryptoAverage> cex, IEnumerable<CryptoAverage> dex)
    {
        return cex.Concat(dex).Aggregate(
            new Dictionary<string, Average>(),
            (acc, curr) =>
            {
                if (!acc.TryGetValue(curr.currency_pair, out var average))
                {
                    average = new Average()
                    {
                        buy = new AverageSide() { money = 0, price = 0, volume = 0 },
                        sell = new AverageSide() { money = 0, price = 0, volume = 0 },
                    };

                    acc.Add(curr.currency_pair, average);
                }

                var side = curr.side.ToLower() == "buy" ? average.buy : average.sell;

                side.money += curr.total_money;
                side.volume += curr.total_volume;
                side.price = side.money / side.volume;

                return acc;
            });
    }

    public List<Order> CombineCexWithDexOrders(IEnumerable<Order> cexOrders, IEnumerable<Order> dexOrders)
    {
        var dexOrdersArr = dexOrders.ToArray();
        var result = new List<Order>();
        var dexIndex = 0;

        foreach (var cexOrder in cexOrders)
        {
            while (dexOrdersArr.Length > dexIndex && dexOrdersArr[dexIndex].updateTimestamp > cexOrder.updateTimestamp)
            {
                result.Add(dexOrdersArr[dexIndex]);
                dexIndex++;
            }


            result.Add(cexOrder);
        }

        if (dexIndex < dexOrdersArr.Length)
        {
            for (var i = dexIndex; i < dexOrdersArr.Length; i++)
            {
                result.Add(dexOrdersArr[i]);
            }
        }

        return result;
    }
}

