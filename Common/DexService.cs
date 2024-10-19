using Common.Interfaces;
using Common.Models;

namespace Common;

public class DexService : IDexService
{
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

