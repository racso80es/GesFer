namespace GesFer.Shared.Back.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    string UserName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
