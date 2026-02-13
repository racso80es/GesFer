namespace GesFer.Admin.Application.Dtos.Logs;

public class LogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? Source { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? UserId { get; set; }
}
