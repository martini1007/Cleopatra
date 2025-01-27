using System.ComponentModel.DataAnnotations;

namespace Cleopatra.Models;

public class AddProductModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Brand { get; set; }
    public int QuantityInStock { get; set; } = 0;
    public double PricePerUnit { get; set; } = 0;
    public string LastRestockedDate { get; set; } = DateTime.Now.ToString("s");
}