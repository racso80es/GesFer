namespace GesFer.ConsoleApp.Services.Interfaces;

public interface IAuditorService
{
    bool ValidateToken(string token);
    void LogAccess(string action, string user, string result, string details = "");
}
