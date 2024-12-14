namespace Cleopatra.Data;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public int QuantityInStock { get; set; }
    public double PricePerUnit { get; set; }
    public DateTime? LastRestockedDate { get; set; }
}