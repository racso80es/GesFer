namespace GesFer.Admin.Application.Dtos.Logs;

public class PurgeLogsResponseDto
{
    public int DeletedCount { get; set; }
    public DateTime DateLimit { get; set; }
}
