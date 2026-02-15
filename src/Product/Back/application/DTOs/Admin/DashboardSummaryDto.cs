namespace GesFer.Application.DTOs.Admin;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalArticles { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public DateTime GeneratedAt { get; set; }
}
