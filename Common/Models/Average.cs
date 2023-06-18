namespace Common.Models;

public class Average
{
    public AverageSide buy { get; set; }
    public AverageSide sell { get; set; }
}

public class AverageSide
{
    public double money { get; set; }
    public double volume { get; set; }
    public double price { get; set; }
}

