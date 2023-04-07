using CommonProduct = Common.Models.Product;

namespace ByBit.Models;

class Product
{
    public string symbol { get; set; }
    public LotSizeFilter lotSizeFilter { get; set; }
    public PriceFilter priceFilter { get; set; }

    public CommonProduct ToCommonProduct()
    {
        return new CommonProduct()
        {
            currencyPair = this.symbol,
            minQuantity = decimal.Parse(this.lotSizeFilter.minOrderQty),
            minTotal = decimal.Parse(this.lotSizeFilter.minOrderAmt),
            pricePrecision = double.Parse(this.priceFilter.tickSize),
        };
    }
}

class LotSizeFilter
{
    public string basePrecision { get; set; }
    public string minOrderQty { get; set; }     // coins amount
    public string minOrderAmt { get; set; }     // price total
}

class PriceFilter
{
    public string tickSize { get; set; }
}