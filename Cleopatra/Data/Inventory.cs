namespace Cleopatra.Data;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } // Navigation property
    public int? QuantityUsed { get; set; }
    public DateTime UsedDate { get; set; }
}