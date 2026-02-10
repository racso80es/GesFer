using MediatR;

namespace GesFer.Admin.Application.Commands.Company;

public record DeleteCompanyCommand(Guid Id) : IRequest;
