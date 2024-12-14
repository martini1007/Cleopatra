namespace Cleopatra.Data;

public class Report
{
    public int ReportId { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string Type { get; set; }
    public string FilePath { get; set; }
}