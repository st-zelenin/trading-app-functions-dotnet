namespace Common.Models;

public class OrderedSymbols
{
    public IEnumerable<string> symbols { get; set; }
    public string exchange { get; set; }
}