namespace GesFer.Admin.Application.Dtos.Logs;

public class CreateLogDto
{
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime TimeStamp { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
