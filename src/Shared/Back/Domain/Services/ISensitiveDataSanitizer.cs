namespace GesFer.Shared.Back.Domain.Services;

public interface ISensitiveDataSanitizer
{
    string GenerateRandomPassword(int length = 12);
    string GenerateRandomEmail(string? prefix = null, string domain = "gesfer.local");
    string Sanitize(string input);
}
