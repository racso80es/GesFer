using MediatR;

namespace GesFer.Shared.Back.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
