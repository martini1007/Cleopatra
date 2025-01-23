namespace Cleopatra.Data;

public class Customer
{
    public int CustomerId { get; set; }
    public string IdentityUserId { get; set; } // Optional foreign key to IdentityUser
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedDate { get; set; }
}