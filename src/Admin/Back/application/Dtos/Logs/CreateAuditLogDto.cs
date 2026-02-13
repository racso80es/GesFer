namespace GesFer.Admin.Application.Dtos.Logs;

public class CreateAuditLogDto
{
    public string CursorId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
    public DateTime ActionTimestamp { get; set; }
}
