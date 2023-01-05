using System;
using System.Collections.Generic;
using System.Linq;
using CommonProduct = Common.Models.Product;

namespace Binance.Models;

public class ExchangeInfo<T> where T: BaseProduct
{
    public IEnumerable<T>  symbols { get; set; }
}

public class BaseProduct
{
    public string symbol { get; set; }      // "BTCUSDT"
    public string quoteAsset { get; set; }  // "USDT"
    public string status { get; set; }      // "TRADING"
}

public class Product : BaseProduct
{
    public string baseAsset { get; set; }           // "BTC"
    public int quoteAssetPrecision { get; set; }    // 8
    public IEnumerable<ProductFilter> filters { get; set; }

    public CommonProduct ToCommonProduct()
    {
        var minNotional = this.filters.FirstOrDefault(f => f.filterType == "MIN_NOTIONAL");
        var minTotal = minNotional == null ? 0 : decimal.Parse(minNotional.minNotional);

        var lotSize = this.filters.FirstOrDefault(f => f.filterType == "LOT_SIZE");
        var minQuantity = lotSize == null ? 0 : decimal.Parse(lotSize.minQty);

        var priceFilter = this.filters.FirstOrDefault(f => f.filterType == "PRICE_FILTER");
        var pricePrecision = priceFilter == null ? 0 : double.Parse(priceFilter.minPrice);

        return new CommonProduct()
        {
            currencyPair = this.symbol,
            minQuantity = minQuantity,
            minTotal = minTotal,
            pricePrecision = pricePrecision,
        };
    }
}

public class ProductFilter
{
    public string filterType { get; set; }
    public string minNotional { get; set; }
    public string minQty { get; set; }
    public string minPrice { get; set; }
}
