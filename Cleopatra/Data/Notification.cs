namespace Cleopatra.Data;

public class Notification
{
    public int NotificationId { get; set; }
    public int? CustomerId { get; set; }
    public Customer Customer { get; set; } // Navigation property
    public string Type { get; set; }
    public string Message { get; set; }
    public DateTime SentDate { get; set; }
    public string Status { get; set; }
}