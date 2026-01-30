namespace GesFer.Application.DTOs.Admin;

/// <summary>
/// DTO para el resumen del dashboard administrativo
/// </summary>
public class DashboardSummaryDto
{
    public int TotalCompanies { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalArticles { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
